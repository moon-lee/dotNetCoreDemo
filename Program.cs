using System;
using System.IO;

namespace dotNetCoreDemo {
    class Program {

        public static string datafile = "data.txt";
        static void Main (string[] args) {
            using (var fs = new FileStream (datafile, FileMode.Open, FileAccess.ReadWrite)) {
                using (var sr = new StreamReader (fs)) {
                    string line;
                    while ((line = sr.ReadLine ()) != null) {
                        // process the line

                        readNode(line);
                        Console.WriteLine(line);
                    }
                }
            }
          
        }

        public static bool readNode(String node) {
            return true;
        }
    }
}