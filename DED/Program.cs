namespace CherrySoft.Utilities.DED
{
    #region Includes
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    #endregion

    public class Program
    {
        #region Constants
        public const string DED_TITLE = "Delete Empty Directories \"DED\" v2.0";
        public const string DED_SOURCE = "by RiA, November 2020";

        public const string ARG_NODELETE = "-nodel";
        public const string ARG_SINGLEPASS = "-1pass";
        public const string ARG_VERBOSESHY = "-shy";
        public const string ARG_SHOWHELP = "/?";

        public const string DESC_NODELETE = "Analysis mode. Disable deletion of directories.";
        public const string DESC_SINGLEPASS = "Single Pass. Ignore parents of empty directories.";
        public const string DESC_VERBOSESHY = "Minimal console output. Hide detailed verbose messages.";

        public const string LABEL_TARGETPATH = "Target Path";
        public const string LABEL_UNKNOWNARGS = "Unknown arguments";
        public const string LABEL_FOUND = "Found";
        public const string LABEL_EMPTY = "Empty";
        public const string LABEL_FOUND_D = "Found directories";
        public const string LABEL_DELETED = "Deleted";
        public const string LABEL_DENIED = "Denied";
        public const string LABEL_FOUNDREPORT = "Empty directories found";
        public const string LABEL_DELETEDREPORT = "Empty directories deleted";
        public const string LABEL_FAILEDREPORT = "Deletions failed";

        public const string XC = ": ";
        public const string XQ = "? ";
        public const string XH = " - ";
        public const string XS = "   ";
        public const string X_B1 = "[";
        public const string X_B0 = "]";
        public const string X_CHECKED = "[*]";
        public const string X_UNCHECKED = "[ ]";

        public const string DIALOG_REPEATPROGRAM = "Repeat program";
        public const string DIALOG_NODELETE = "Disable Deletion of Directories";
        public const string DIALOG_SINGLEPASS = "Ignore parents of empty directories";
        public const string DIALOG_VERBOSESHY = "Hide detailed verbose messages";

        public const string MESSAGE_SCANSTART = "Starting scan...";
        public const string MESSAGE_SCANEND = "Scan Completed";
        public const string MESSAGE_SCANFAILED = "Scan failed on target";
        public const string MESSAGE_FOUND_ED0 = "Target site does not contain any empty directories.";
        public const string MESSAGE_FOUND_EDS0 = "Target site does not contain any empty directory structures.";
        public const string MESSAGE_FOUND_0 = "Target site does not contain any directories.";
        public const string MESSAGE_EXITPROGRAM = "Exiting Program...";

        public const string RESPONSE_YND = "(y/n, default: n)";
        
        #endregion

        #region Properties
        public static string TargetPath { get; set; } = string.Empty;
        public static bool NoDelete { get; set; } = false;
        public static bool SinglePass { get; set; } = false;
        public static bool VerboseShy { get; set; } = false;

        #endregion

        public enum ExecutionMode : byte
        {
            Abort = 0,
            Interactive = 1,
            Unattended = 2,
        }

        #region Main
        public static void Main(string[] args)
        {
            // Store default console settings
            var b = Console.BackgroundColor;
            var f = Console.ForegroundColor;

            // Intro
            Introduce();

            // Parse Arguments
            var executionMode = ArgsParse(args);

            if (executionMode == ExecutionMode.Unattended)
            {
                // Unattended Routine
                ProcessDirectories();
            }
            else if (executionMode == ExecutionMode.Interactive)
            {
                // Interactive Routine
                do
                {
                    InteractiveInput();
                    ProcessDirectories();
                    MessageOut();
                    MessageOut(DIALOG_REPEATPROGRAM + XQ + RESPONSE_YND);
                }
                while (ConfirmByUser());
            }

            Conclude();

            // Restore default console settings
            Console.BackgroundColor = b;
            Console.ForegroundColor = f;
        }
        #endregion

        #region Input: Arguments Parser
        public static ExecutionMode ArgsParse(string[] args)
        {
            ExecutionMode executionMode = ExecutionMode.Unattended;

            // If path was given then automatic routine, else interactive routine
            if (args.Length < 2)
            {
                executionMode = ExecutionMode.Interactive;
                goto preReturn;
            }
            else if (args[1] == ARG_SHOWHELP)
            {
                ShowHelp();
                executionMode = ExecutionMode.Abort;
                goto preReturn;
            }
            else
            {
                VerboseOut(LABEL_TARGETPATH + XC + args[1]);
                TargetPath = args[1];
            }

            // Process Switches
            if (args.Length < 3)
            {
                goto preReturn;
            }
            List<string> argsub = new List<string>(args.Skip(2));

            // NoDelete, SinglePass, VerboseShy
            NoDelete = argsub.Contains(ARG_NODELETE, StringComparer.OrdinalIgnoreCase);
            SinglePass = argsub.Contains(ARG_SINGLEPASS, StringComparer.OrdinalIgnoreCase);
            VerboseShy = argsub.Contains(ARG_VERBOSESHY, StringComparer.OrdinalIgnoreCase);

            // Remove this block if arguments display is mandatory despite 'VerboseShy == true'
            if (VerboseShy)
            {
                goto preReturn;
            }

            // Prepare for verbose intro: Isolate remaining unknown arguments
            argsub.RemoveAll(n => n.Equals(ARG_NODELETE, StringComparison.OrdinalIgnoreCase));
            argsub.RemoveAll(n => n.Equals(ARG_SINGLEPASS, StringComparison.OrdinalIgnoreCase));
            argsub.RemoveAll(n => n.Equals(ARG_VERBOSESHY, StringComparison.OrdinalIgnoreCase));

            // Display Verbose
            VerboseOut(XS + (NoDelete ? X_CHECKED : X_UNCHECKED) + XH + ARG_NODELETE + XC + DESC_NODELETE);
            VerboseOut(XS + (SinglePass ? X_CHECKED : X_UNCHECKED) + XH + ARG_SINGLEPASS + XC + DESC_SINGLEPASS);
            VerboseOut(XS + (VerboseShy ? X_CHECKED : X_UNCHECKED) + XH + ARG_VERBOSESHY + XC + DESC_VERBOSESHY);

            if (argsub.Count() > 0)
            {
                VerboseOut("[?] " + LABEL_UNKNOWNARGS + XC + string.Join(" ", argsub));
            }

        preReturn:
            return executionMode;
        }
        #endregion

        #region Input: User Interactive
        public static void InteractiveInput()
        {
            MessageOut();
            MessageOut(LABEL_TARGETPATH + XC);
            TargetPath = GetUserInput();

            MessageOut();
            MessageOut(DIALOG_NODELETE + XQ + RESPONSE_YND);
            NoDelete = ConfirmByUser();

            MessageOut();
            MessageOut(DIALOG_SINGLEPASS + XQ + RESPONSE_YND);
            SinglePass = ConfirmByUser();

            MessageOut();
            MessageOut(DIALOG_VERBOSESHY + XQ + RESPONSE_YND);
            VerboseShy = ConfirmByUser();
        }
        #endregion

        #region Processing: Router
        public static void ProcessDirectories()
        {
            if(NoDelete)
            {
                AnalyseDirectories();
            }
            else
            {
                DeleteDirectories();
            }
        }
        #endregion

        #region Processing: Analysis
        public static void AnalyseDirectories()
        {
            ulong foundCount = 0;
            string[] allDirectories;
            List<string> emptyDirectories = new List<string>(0);

            // Check if target site can be accessed, or has processable elements
            VerboseOut(Environment.NewLine + MESSAGE_SCANSTART + XC);
            allDirectories = ScanDirectories();
            if (allDirectories == null)
            {
                return;
            }

            // Process directories
            VerboseOut();
            foreach (string path in allDirectories)
            {
                try
                {
                    if ((SinglePass ? Directory.GetFileSystemEntries(path).Length : Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length) == 0)
                    {
                        if (!emptyDirectories.Contains(path, StringComparer.OrdinalIgnoreCase))
                        {
                            emptyDirectories.Add(path);
                            foundCount++;
                            VerboseOut(XH + LABEL_FOUND + XC + path);
                        }
                    }
                }
                catch (Exception e)
                {
                    VerboseOut(XH + LABEL_DENIED + XC + path);
                    ErrorOut(e.Source + XC + e.Message);
                }
            }

            // Verbose summary
            if (foundCount != 0)
            {
                VerboseOut();
            }
            VerboseOut(XS + X_B1 + LABEL_EMPTY + XC + foundCount + X_B0);

            // Finalize and display report
            MessageOut(Environment.NewLine + MESSAGE_SCANEND + Environment.NewLine);
            if (emptyDirectories.Count() == 0)
            {
                if(SinglePass)
                {
                    ErrorOut(MESSAGE_FOUND_ED0);
                }
                else
                {
                    ErrorOut(MESSAGE_FOUND_EDS0);
                }
            }
            else
            {
                MessageOut("Multipass" + XC + (SinglePass ? "No" : "Yes"));
                MessageOut(LABEL_FOUNDREPORT + XC + emptyDirectories.Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString());
            }
        }
        #endregion

        #region Processing: Delete
        public static void DeleteDirectories()
        {
            ulong passCount = 0;
            ulong foundCount;
            ulong deletedCount;

            string[] allDirectories;
            List<string> emptyDirectories = new List<string>(0);
            List<string> deletedDirectories = new List<string>(0);

            do
            {
                VerboseOut();

                // Reset cyclic stats
                foundCount = 0;
                deletedCount = 0;

                // Check if target site can be accessed, or has processable elements
                VerboseOut(Environment.NewLine + (SinglePass ? MESSAGE_SCANSTART : ("Pass " + (passCount + 1).ToString())) + XC);
                allDirectories = ScanDirectories();
                if (allDirectories == null)
                {
                    break;
                }

                // Start processing
                if (allDirectories.Length != 0)
                {
                    VerboseOut();
                    foreach (string path in allDirectories)
                    {
                        try
                        {
                            if (Directory.GetFileSystemEntries(path).Length == 0)
                            {
                                // Register for found
                                emptyDirectories.Add(path);
                                foundCount++;

                                // Attempt deletion
                                Directory.Delete(path);
                                deletedDirectories.Add(path);
                                deletedCount++;
                                VerboseOut(XH + LABEL_DELETED + XC + path);
                            }
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            VerboseOut(XH + LABEL_DENIED + XC + path);
                            ErrorOut(e.Source + XC + e.Message);
                        }
                    }
                }

                passCount++;

                // Verbose summary for this pass
                if (foundCount != 0)
                {
                    VerboseOut();
                }   
                VerboseOut(XS + X_B1 + LABEL_EMPTY + XC + foundCount + "; " + LABEL_DELETED + XC + deletedCount + X_B0);
            }
            while (!SinglePass && deletedCount != 0);

            // Finalize and display reports
            MessageOut(Environment.NewLine + MESSAGE_SCANEND + Environment.NewLine);
            if (emptyDirectories.Count() == 0)
            {
                if (SinglePass)
                {
                    ErrorOut(MESSAGE_FOUND_ED0);
                }
                else
                {
                    ErrorOut(MESSAGE_FOUND_EDS0);
                }
            }
            else
            { 
                // Report: Passes
                if (!SinglePass)
                {
                    MessageOut("Passes" + XC + (passCount > 1 ? passCount - 1 : passCount).ToString());
                }
                // Report: Found count
                var fcount = emptyDirectories.Distinct().Count();
                MessageOut(LABEL_FOUNDREPORT + XC + fcount.ToString());

                if (!NoDelete)
                {
                    // Report: Deleted count
                    var dcount = deletedDirectories.Distinct().Count();
                    MessageOut(LABEL_DELETEDREPORT + XC + dcount.ToString());

                    // Report: Failed count
                    var xcount = fcount - dcount;
                    if (xcount != 0)
                    {
                        ErrorOut(LABEL_FAILEDREPORT + XC + xcount.ToString());
                    }
                }
            }
        }
        #endregion

        #region Processing: Scan
        public static string[] ScanDirectories()
        {   
            try
            {
                string[] allDirectories = Directory.GetDirectories(Path.GetFullPath(TargetPath), "*", SearchOption.AllDirectories);
                if (allDirectories.Length == 0)
                {
                    ErrorOut();
                    ErrorOut(MESSAGE_FOUND_0);
                    return null;
                }
                else
                {
                    VerboseOut();
                    VerboseOut(XS + X_B1 + LABEL_FOUND_D + XC + allDirectories.Length + X_B0);
                    return allDirectories;
                }
            }
            catch (Exception e)
            {
                ErrorOut();
                ErrorOut(XH + e.Source + XC + e.Message);
                ErrorOut(Environment.NewLine + MESSAGE_SCANFAILED + XC + TargetPath);
                return null;
            }
        }
        #endregion

        #region Utilities: Input
        public static string GetUserInput()
        {
            Console.ForegroundColor = ConsoleColor.White;
            string inputData = Console.ReadLine();
            return inputData;
        }
        public static bool ConfirmByUser()
        {
            var userInput = GetUserInput();
            return userInput.Equals("y", StringComparison.OrdinalIgnoreCase)
                || userInput.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Utilities: Output
        public static void MessageOut(string message = default)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }
        public static void VerboseOut(string message = default)
        {
            if (VerboseShy)
            {
                return;
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(message);
        }
        public static void ErrorOut(string message = default)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
        }
        #endregion

        #region Utilities: Misc
        public static void Introduce() => MessageOut(Environment.NewLine + Environment.NewLine + DED_TITLE + Environment.NewLine + DED_SOURCE + Environment.NewLine);
        public static void Conclude() => MessageOut(Environment.NewLine + MESSAGE_EXITPROGRAM + Environment.NewLine + Environment.NewLine);
        public static void ShowHelp()
        {
            MessageOut("Usage" + XC + Environment.NewLine);
            MessageOut("ded DirectoryPath -switch1 -switch2 -switch3" + Environment.NewLine);
            MessageOut("Switches" + Environment.NewLine);
            MessageOut(); //TODO
        }
        #endregion
    }
}
