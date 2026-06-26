using System;
using System.IO;
using System.Text;
using ConfigEditor.Core.Common;
using ConfigEditor.Core.Interfaces;

namespace ConfigEditor.Infrastructure;

/// <summary>
/// Service to perform safe file operations, check permissions, and detect encoding.
/// </summary>
public class FileService : IFileService
{
    public Result<string> ReadAllText(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return Result<string>.Failure("FILE_NOT_FOUND", $"File not found at: {filePath}");

            byte[] bytes = File.ReadAllBytes(filePath);
            Encoding encoding = DetectEncoding(bytes);

            string text = encoding.GetString(bytes);
            return Result<string>.Success(text);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure("READ_FAILED", $"Failed to read file: {ex.Message}");
        }
    }

    public Result WriteAllText(string filePath, string content, Encoding? encoding = null)
    {
        try
        {
            if (encoding == null)
            {
                if (File.Exists(filePath))
                {
                    byte[] bytes = File.ReadAllBytes(filePath);
                    encoding = DetectEncoding(bytes);
                }
                else
                {
                    encoding = new UTF8Encoding(false);
                }
            }

            File.WriteAllText(filePath, content, encoding);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("WRITE_FAILED", $"Failed to write file: {ex.Message}");
        }
    }

    public bool Exists(string filePath)
    {
        return File.Exists(filePath);
    }

    public bool IsReadOnly(string filePath)
    {
        if (!File.Exists(filePath))
            return false;
        var info = new FileInfo(filePath);
        return info.IsReadOnly;
    }

    public Result CheckWritePermission(string filePath)
    {
        try
        {
            if (IsReadOnly(filePath))
                return Result.Failure("READ_ONLY", "File is read-only.");

            if (File.Exists(filePath))
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    // Opened successfully for writing
                }
            }
            else
            {
                string? dir = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(dir))
                    dir = Directory.GetCurrentDirectory();

                if (!Directory.Exists(dir))
                    return Result.Failure("DIRECTORY_NOT_FOUND", $"Directory does not exist: {dir}");

                string tempFile = Path.Combine(dir, Guid.NewGuid().ToString() + ".tmp");
                using (var fs = File.Create(tempFile)) { }
                File.Delete(tempFile);
            }
            return Result.Success();
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Failure("NO_WRITE_PERMISSION", "No write permission to the target location.");
        }
        catch (IOException ex)
        {
            return Result.Failure("FILE_LOCKED", $"File is locked or in use: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure("PERMISSION_CHECK_FAILED", $"Permission check failed: {ex.Message}");
        }
    }

    private Encoding DetectEncoding(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return new UTF8Encoding(true);
        }
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            return Encoding.Unicode;
        }
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode;
        }

        return new UTF8Encoding(false);
    }
}
