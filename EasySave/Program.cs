// See https://aka.ms/new-console-template for more information

using EasyLog;
using EasyLog.entity;

Console.WriteLine("Hello, World!");

Logger logger = new Logger();
logger.InitWriters();
logger.Log("Hello, World!", LogType.Error);
logger.Log("Hello, World!", LogType.Warning);
logger.Log("Hello, World!", LogType.Info);
