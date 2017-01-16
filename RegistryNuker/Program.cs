using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.Win32;
using static Microsoft.Win32.Registry;

namespace RegistryNuker
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("This program will search for a keyword and delete every key that contains it. USE WITH CARE, THIS CAN AND WILL WRECK WINDOWS.");
            Console.WriteLine("Enter a number: ");
            Console.WriteLine("1: CurrentUser");
            Console.WriteLine("2: LocalMachine");
            
            RegistryKey basekey = null;
            while (basekey == null)
            {
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        basekey = CurrentUser;
                        break;
                    case "2":
                        basekey = LocalMachine;
                        break;
                    default:
                        Console.WriteLine("Invalid number");
                        break;
                }
            }

            Console.WriteLine("Select a mode?");
            Console.WriteLine("Enter a number: ");
            Console.WriteLine("1: Dry run - list all matches");
            Console.WriteLine("2: Manual mode - list and ask if it should delete");
            Console.WriteLine("3: Nuclear mode - delete all matches without confirmation");
            var shouldDelete = false;
            var confirmation = true;
            var haveShouldDelete = false;
            while (haveShouldDelete == false)
            {
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        shouldDelete = false;
                        confirmation = false;
                        haveShouldDelete = true;
                        break;
                    case "2":
                        shouldDelete = true;
                        confirmation = true;
                        haveShouldDelete = true;
                        break;
                    case "3":
                        shouldDelete = true;
                        confirmation = false;
                        haveShouldDelete = true;
                        break;
                    default:
                        Console.WriteLine("Invalid number");
                        break;
                }
            }

            Console.WriteLine("Enter a search string to delete matches for. Not case sensitive. BE CAREFUL!!!");
            var searchstring = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(searchstring))
            {
                Console.WriteLine("Not going to delete everything. Goodbye");
                return;
            }

            GetValuesContaining(searchstring, basekey, shouldDelete, confirmation);
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadLine();
        }

        private static void GetValuesContaining(string searchString, RegistryKey key, bool delete = false, bool confirmation = true, bool skipThis = false)
        {
            // Repeat for all subkeys
            var subkeyNames = key.GetSubKeyNames();
            foreach (var subkeyName in subkeyNames)
            {
                try
                {
                    var subkey = key.OpenSubKey(subkeyName, true);
                    GetValuesContaining(searchString, subkey, delete, confirmation);
                }
                catch (Exception)
                {
                    try
                    {
                        var subkey = key.OpenSubKey(subkeyName);
                        GetValuesContaining(searchString, subkey, delete, confirmation, true);
                    }
                    catch (Exception)
                    {
                        // Well shit.
                    }
                }
            }

            if (skipThis) return;

            // Get values containing
            var valueNames = key.GetValueNames();
            foreach (var valueName in valueNames)
            {
                var value = key.GetValue(valueName, true);
                
                // Ignore miss
                if (!value.ToString().ToLower().Contains(searchString.ToLower())) continue;

                Console.WriteLine($"{key.Name} | {valueName} | {value}");

                // List mode
                if (!delete) continue;

                // Delete mode
                var shouldDelete = true;
                if (confirmation)
                {
                    Console.WriteLine("Delete? Y/n");
                    var responded = false;
                    while (!responded)
                    {
                        var input = Console.ReadLine();
                        switch (input.ToLower())
                        {
                            case "n":
                                shouldDelete = false;
                                responded = true;
                                break;
                            case "y":
                                shouldDelete = true;
                                responded = true;
                                break;
                            default:
                                Console.WriteLine("Invalid answer");
                                break;
                        }
                    }
                }
                if (shouldDelete) key.DeleteValue(valueName);
            }
        }
    }
}
