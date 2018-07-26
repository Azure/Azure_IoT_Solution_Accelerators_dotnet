// Copyright (c) Microsoft. All rights reserved.

using System.IO;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage
{
    public interface IFileWrapper
    {
        string GetTempFileName();
        void WriteLine(StreamWriter file, string line);
        StreamWriter GetStreamWriter(string fileName);
        void Delete(string fileName);
        bool Exists(string fileName);
        void WriteAllText(string fileName, string content);
    }

    public class FileWrapper : IFileWrapper
    {
        public StreamWriter GetStreamWriter(string fileName)
        {
            return new StreamWriter(fileName);
        }

        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        public void WriteLine(StreamWriter file, string line)
        {
            file.WriteLine(line);
        }

        public void Delete(string fileName)
        {
            File.Delete(fileName);
        }

        public bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }

        public void WriteAllText(string fileName, string content)
        {
            File.WriteAllText(fileName, content);
        }
    }
}
