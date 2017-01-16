using System;
using System.Linq;
using Microsoft.Win32;
using static Microsoft.Win32.Registry;

namespace RegistryNuker
{
    internal class RegistryNuker
    {
        private static SearchMode searchMode;
        private static DeleteMode deleteMode;
        private static string searchString;

        private enum SearchMode
        {
            ValueNames,
            ValueData,
            Both
        }

        private enum DeleteMode
        {
            Readonly,
            ConfirmBeforeDelete,
            DeleteAll
        }

        private static void Main()
        {
            // Select a base key
            Console.WriteLine("This program will search for a keyword and delete every key that contains it. USE WITH CARE, THIS CAN AND WILL WRECK WINDOWS.");
            Console.WriteLine("Select a base key to search through: ");
            Console.WriteLine("1: CurrentUser");
            Console.WriteLine("2: LocalMachine");
            
            RegistryKey basekey = null;
            var basekeyInput = GetInput(new[] { "1", "2" });
            switch (basekeyInput)
            {
                case "1": basekey = CurrentUser;  break;
                case "2": basekey = LocalMachine; break;
            }

            // Mode selection
            Console.WriteLine("Select a mode:");
            Console.WriteLine("1: Dry run      - list all matches");
            Console.WriteLine("2: Manual mode  - asks if you want to delete each key");
            Console.WriteLine("3: Nuclear mode - delete all matches without confirmation");
            var modeInput = GetInput(new[] { "1", "2", "3" });
            switch (modeInput)
            {
                case "1": deleteMode = DeleteMode.Readonly;            break;
                case "2": deleteMode = DeleteMode.ConfirmBeforeDelete; break;
                case "3": deleteMode = DeleteMode.DeleteAll;           break;
            }

            // Search string
            Console.WriteLine("Enter a search string to delete matches for. Not case sensitive. BE CAREFUL!!!");
            searchString = GetInput().ToLowerInvariant();
            
            // Search mode
            Console.WriteLine("Select a search mode.");
            Console.WriteLine("1: Search in value names");
            Console.WriteLine("2: Search in value data");
            Console.WriteLine("3: Search in both");
            var searchmodeInput = GetInput(new[] {"1", "2", "3"});
            switch (searchmodeInput)
            {
                case "1": searchMode = SearchMode.ValueNames; break;
                case "2": searchMode = SearchMode.ValueData;  break;
                case "3": searchMode = SearchMode.Both;       break;
            }

            // Start searching
            GetValuesContaining(basekey);
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Calls itself on all subkeys and looks for values matching the search string. Uses all three static variables but does not change them.
        /// </summary>
        /// <param name="key">The key to search through.</param>
        /// <param name="thisKeyIsReadonly">Skips searching/deliting values for this key, useful if readonly.</param>
        private static void GetValuesContaining(RegistryKey key, bool thisKeyIsReadonly = false)
        {
            // Repeat recursively for all subkeys
            var subkeyNames = key.GetSubKeyNames();
            foreach (var subkeyName in subkeyNames)
            {
                // Try opening with write access.
                try
                {
                    var subkey = key.OpenSubKey(subkeyName, true);
                    GetValuesContaining(subkey);
                }
                catch (Exception)
                {
                    // Otherwise try opening with read-only access, since we might be able to get write access on subkeys
                    try
                    {
                        var subkey = key.OpenSubKey(subkeyName);
                        GetValuesContaining(subkey, true);
                    }
                    catch (Exception)
                    {
                        // Or not.
                    }
                }
            }

            // Don't bother attempting to delete if this key is readonly.
            if (thisKeyIsReadonly) return;

            // Get values containing search string
            var valueNames = key.GetValueNames();
            foreach (var valueName in valueNames)
            {
                var valueData = key.GetValue(valueName, true).ToString();

                switch (searchMode)
                {
                    case SearchMode.ValueNames:
                        if (!valueName.ToLowerInvariant().Contains(searchString)) continue;
                        break;
                    case SearchMode.ValueData:
                        if (!valueData.ToLowerInvariant().Contains(searchString)) continue;
                        break;
                    case SearchMode.Both:
                        if (!valueData.ToLowerInvariant().Contains(searchString)) continue;
                        if (!valueName.ToLowerInvariant().Contains(searchString)) continue;
                        break;
                }

                // Always print matching values.
                Console.WriteLine($"{key.Name} | {valueName} | {valueData}");

                // Next action depends on selected delete mode.
                switch (deleteMode)
                {
                    case DeleteMode.Readonly:
                        continue;

                    case DeleteMode.ConfirmBeforeDelete:
                        Console.WriteLine("Delete? Y/n");
                        var response = GetInput(new[] {"y", "n"});
                        if (response == "y") key.DeleteValue(valueName);
                        break;

                    case DeleteMode.DeleteAll:
                        key.DeleteValue(valueName);
                        break;
                }
            }
        }

        /// <summary>
        /// Returns user input through Console.ReadLine. Does not accept blank input.
        /// </summary>
        /// <param name="validInputs">A list of valid inputs. If not null, only these answers will be accepted. All strings should be lowercase.</param>
        /// <returns>The user's answer in lowercase.</returns>
        private static string GetInput(string[] validInputs = null)
        {
            while (true)
            {
                var input = Console.ReadLine();
                
                // Don't accept blank input.
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Please enter a valid value.");
                    continue;
                }

                input = input.ToLowerInvariant();

                // Return here if we don't need to check for valid inputs.
                if (validInputs == null) return input;

                // Return if the input passes the validity rules.
                if (validInputs.Contains(input.ToLower())) return input;

                // Otherwise loop again.
                Console.WriteLine("Please enter a valid value.");
            }
        }
    }
}
