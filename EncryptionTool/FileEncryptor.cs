using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace EncryptionTool
{
    public static class FileEncryptor
    {
        // Encrypts an input file and saves it as an encrypted output file
        public static void EncryptFile(
            string inputFilePath,       // Path of the file that will be encrypted
            string outputFilePath,      // Path where the encrypted file will be saved
            string keyString,           // The secret key used for encryption
            Action<double>? progressCallback = null, // Optional: reports encryption progress (0 to 100%)
            Func<bool>? cancelRequested = null,      // Optional: allows canceling encryption
            Action<string>? renameNotification = null) // Optional: notifies if the file is renamed
        {
            // 1) Get the file extension (e.g. ".txt", ".jpg")
            string extension = Path.GetExtension(inputFilePath) ?? "";

            // 2) Convert the provided key into a 256-bit key suitable for AES encryption
            byte[] key = EnsureKeySize(keyString, 32);

            // 3) Generate a random IV (16 bytes) to make encryption more secure
            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv); // Generate random bytes for IV
            }

            // 4) Create the AES encryption object using the key and IV
            IBufferedCipher cipher = CreateAesCbcCipher(key, iv, true);

            // 5) Open the input file for reading and create an output file for writing
            using (FileStream inputFs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream outputFs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                // 6) Convert the file extension to bytes so we can store it inside the encrypted file
                byte[] extBytes = Encoding.UTF8.GetBytes(extension);
                int extLen = extBytes.Length;

                // 7) Write the length of the file extension (4 bytes)
                byte[] lengthBytes = BitConverter.GetBytes(extLen);
                outputFs.Write(lengthBytes, 0, lengthBytes.Length);

                // 8) If there is an extension (e.g. ".txt"), write it to the output file
                if (extLen > 0)
                    outputFs.Write(extBytes, 0, extBytes.Length);

                // 9) Write the IV (16 bytes) to the output file
                outputFs.Write(iv, 0, iv.Length);

                // 10) Read the input file in chunks to encrypt it piece by piece
                long totalBytes = inputFs.Length; // Total file size
                long totalBytesRead = 0;          // Bytes read so far
                byte[] buffer = new byte[1024 * 64]; // Buffer size (64 KB)
                int bytesRead;

                // 11) Read the input file and encrypt each chunk
                while ((bytesRead = inputFs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // If the user requested cancellation, stop encryption
                    if (cancelRequested?.Invoke() == true)
                        throw new OperationCanceledException("Encryption canceled.");

                    // 12) Encrypt the current chunk of data
                    byte[] outBuffer = cipher.ProcessBytes(buffer, 0, bytesRead);

                    // 13) Write the encrypted chunk to the output file
                    outputFs.Write(outBuffer, 0, outBuffer.Length);

                    // 14) Update the encryption progress (if a callback function was provided)
                    totalBytesRead += bytesRead;
                    progressCallback?.Invoke((double)totalBytesRead / totalBytes);
                }

                // 15) Finalize the encryption and write the last bytes
                byte[] finalBlock = cipher.DoFinal();
                outputFs.Write(finalBlock, 0, finalBlock.Length);

                // 16) Encryption is complete, update progress to 100%
                progressCallback?.Invoke(1.0);
            }
        }


        // Reads the header, extracts extension and IV, then decrypts the rest.
        // Returns the final path if you need it.
        public static string DecryptFile(
    string inputFilePath,
    string baseOutputPath,
    string keyString,
    Action<double>? progressCallback = null,
    Func<bool>? cancelRequested = null,
    Action<string>? renameNotification = null)
        {
            byte[] key = EnsureKeySize(keyString, 32);

            using (FileStream inputFs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                // read extension length
                byte[] lengthBytes = new byte[4];
                if (inputFs.Read(lengthBytes, 0, 4) < 4)
                    throw new CryptographicException("Invalid .crpt header (missing extension length).");

                int extLen = BitConverter.ToInt32(lengthBytes, 0);
                if (extLen < 0 || extLen > 256)
                    throw new CryptographicException("Invalid extension length in .crpt header.");

                // read extension string
                string originalExtension = "";
                if (extLen > 0)
                {
                    byte[] extBuffer = new byte[extLen];
                    int extRead = inputFs.Read(extBuffer, 0, extLen);
                    if (extRead < extLen)
                        throw new CryptographicException("Incomplete extension data in .crpt header.");
                    originalExtension = Encoding.UTF8.GetString(extBuffer);
                }

                // read IV
                byte[] iv = new byte[16];
                if (inputFs.Read(iv, 0, iv.Length) < 16)
                    throw new CryptographicException("Invalid .crpt header (missing IV).");

                IBufferedCipher cipher = CreateAesCbcCipher(key, iv, false);

                // build final output (e.g. baseOutputPath="C:\folder\fg-04" + ".bin" => "C:\folder\fg-04.bin"
                string finalOutputPath = baseOutputPath + originalExtension;

                // if it exists, rename
                if (File.Exists(finalOutputPath))
                {
                    string unique = GetUniqueFilePath(finalOutputPath);
                    renameNotification?.Invoke($"File \"{finalOutputPath}\" already exists.\nUsing \"{unique}\" instead.");
                    finalOutputPath = unique;
                }

                // decrypt to finalOutputPath
                using (FileStream outputFs = new FileStream(finalOutputPath, FileMode.Create, FileAccess.Write))
                {
                    long totalBytes = inputFs.Length - (4 + extLen + 16);
                    if (totalBytes < 0)
                        throw new CryptographicException("No encrypted data found.");

                    long totalBytesRead = 0;
                    byte[] buffer = new byte[1024 * 64];
                    int bytesRead;

                    try
                    {
                        while ((bytesRead = inputFs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (cancelRequested?.Invoke() == true)
                                throw new OperationCanceledException("Decryption canceled.");

                            byte[] outBuffer = cipher.ProcessBytes(buffer, 0, bytesRead);
                            outputFs.Write(outBuffer, 0, outBuffer.Length);

                            totalBytesRead += bytesRead;
                            progressCallback?.Invoke((double)totalBytesRead / totalBytes);
                        }
                        byte[] finalBlock = cipher.DoFinal();
                        outputFs.Write(finalBlock, 0, finalBlock.Length);
                        progressCallback?.Invoke(1.0);
                    }
                    catch (InvalidCipherTextException)
                    {
                        throw new CryptographicException("Error decrypting the specified file (bad key?).");
                    }
                }
                return finalOutputPath;
            }
        }

        private static IBufferedCipher CreateAesCbcCipher(byte[] key, byte[] iv, bool forEncryption)
        {
            // 1) Create an AES encryption engine
            var engine = new AesEngine();

            // 2) Wrap the AES engine inside a CBC (Cipher Block Chaining) mode
            var blockCipher = new CbcBlockCipher(engine);

            // 3) Add padding (PKCS7) to handle data that is not a multiple of the block size
            var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());

            // 4) Create a key parameter from the provided key
            var keyParam = new KeyParameter(key);

            // 5) Combine the key with the IV (Initialization Vector)
            var keyParamWithIv = new ParametersWithIV(keyParam, iv);

            // 6) Initialize the cipher:
            //    - If forEncryption = true, it will be used for encryption.
            //    - If forEncryption = false, it will be used for decryption.
            cipher.Init(forEncryption, keyParamWithIv);

            // 7) Return the configured AES cipher object
            return cipher;
        }


        private static byte[] EnsureKeySize(string passphrase, int desiredSize)
        {
            // Create a SHA-256 hashing algorithm instance
            using (var sha256 = SHA256.Create())
            {
                // Convert the passphrase into bytes and generate a 256-bit hash
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
            }
        }


        private static string GetUniqueFilePath(string path)
        {
            // If the file does not exist, return the original path
            if (!File.Exists(path))
                return path;

            // Get the directory of the file, ensuring it's not null
            var directory = Path.GetDirectoryName(path) ?? "";

            // Extract the filename without its extension
            var filenameWithoutExt = Path.GetFileNameWithoutExtension(path);

            // Extract the file extension (e.g., ".txt", ".crpt")
            var extension = Path.GetExtension(path);

            int counter = 1;
            string newPath;

            do
            {
                // Create a new filename with a counter (e.g., "file(1).txt", "file(2).txt")
                string tempFileName = $"{filenameWithoutExt}({counter}){extension}";

                // Combine it with the directory path
                newPath = Path.Combine(directory, tempFileName);

                // Increase the counter for the next iteration if needed
                counter++;
            }
            while (File.Exists(newPath)); // Repeat until a unique filename is found

            return newPath; // Return the unique filename
        }

    }
}
