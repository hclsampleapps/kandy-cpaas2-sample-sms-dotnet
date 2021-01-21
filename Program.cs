using dotenv.net;
using Nancy.Hosting.Self;
using System;
using System.IO;

namespace sms {
  class Program {
    static void Main(string[] args) {
      var PORT = 8000;
      var uri = $"http://localhost:{PORT}";

      using (var host = new NancyHost(new Uri(uri))) {
        host.Start();
        var isUnixFS = Path.GetPathRoot(Directory.GetCurrentDirectory()) == @"/";
        var upPath = isUnixFS ? @"../../" : @"..\..\";
        string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), upPath));
        string dotenvpath = isUnixFS ? $"{path}/.env" : $"{path}\\.env";
        DotEnv.Config(true, dotenvpath);
        Console.WriteLine("NancyFX Stand alone test application.");
        Console.WriteLine($"Listening on {uri}");
        Console.WriteLine("Press enter to exit the application");
        Console.ReadLine();
      }
    }
  }
}