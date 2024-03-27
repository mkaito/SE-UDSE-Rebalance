using System;
using Sandbox.ModAPI;
using VRage.Utils;

namespace UDSERebalance.Utilities
{
    public static class Config
    {
        public static T ReadFileFromWorldStorage<T>(string filename, Type type)
        {
            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(filename, type))
                {
                    return default(T);
                }

                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(filename, type))
                {
                    var file = reader.ReadToEnd();
                    return string.IsNullOrWhiteSpace(file)
                        ? default(T)
                        : MyAPIGateway.Utilities.SerializeFromXML<T>(file);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole(
                    $"Error in UDSERebalance.Config.ReadFileFromWorldStorage: {e.Message}\n{e.StackTrace}");
                return default(T);
            }
        }

        public static T ReadFileFromLocalStorage<T>(string filename, Type type)
        {
            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInLocalStorage(filename, type))
                {
                    return default(T);
                }

                using (var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(filename, type))
                {
                    var file = reader.ReadToEnd();
                    return string.IsNullOrWhiteSpace(file)
                        ? default(T)
                        : MyAPIGateway.Utilities.SerializeFromXML<T>(file);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole(
                    $"Error in UDSERebalance.Config.ReadFileFromLocalStorage: {e.Message}\n{e.StackTrace}");
                return default(T);
            }
        }

        public static void WriteFileToWorldStorage<T>(string filename, Type type, T data)
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(filename, type))
                {
                    MyAPIGateway.Utilities.DeleteFileInWorldStorage(filename, type);
                }

                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, type))
                {
                    var config = MyAPIGateway.Utilities.SerializeToXML(data);
                    writer.Write(config);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole(
                    $"Error in UDSERebalance.Config.WriteFileToWorldStorage: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void WriteFileToLocalStorage<T>(string filename, Type type, T data)
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInLocalStorage(filename, type))
                {
                    MyAPIGateway.Utilities.DeleteFileInLocalStorage(filename, type);
                }

                using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, type))
                {
                    var config = MyAPIGateway.Utilities.SerializeToXML(data);
                    writer.Write(config);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole(
                    $"Error in UDSERebalance.Config.WriteFileToLocalStorage: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}