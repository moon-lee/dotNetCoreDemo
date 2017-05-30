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
                        ReadNode(line);
                        Console.WriteLine(line);
                    }
                }
            }
          
        }

        public static bool ReadNode(String node) {
            var delimiters = new char[] {'\t'};
            var items = node.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }

            return true;
        }
    }
}