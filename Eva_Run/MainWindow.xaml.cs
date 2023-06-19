using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/* Author: Denis Gavriusev */

namespace sas_run
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        class ListItem
        {
            public string Program { get; set; }
            public string Status { get; set; }
        }
        private void ClickOpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                var lines = File.ReadAllLines(openFileDialog.FileName);
                foreach (string line in lines)
                { listView1.Items.Add(new ListItem() { Program = line.ToString(), Status = "" }); }
            }
        }
        private void ClickSelectFiles(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "SAS files (*.sas)|*.sas";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string file in openFileDialog.FileNames)
                { listView1.Items.Add(new ListItem() { Program = file, Status = "" }); }
            }
        }
        private void ClickRemove(object sender, RoutedEventArgs e)
        {
            var selected = listView1.SelectedItems.Cast<Object>().ToArray();
            foreach (var item in selected) listView1.Items.Remove(item);
        }
        private void ClickClean(object sender, RoutedEventArgs e)
        {
            listView1.Items.Clear();
        }
        private void ClickRun(object sender, RoutedEventArgs e)
        {
            string saspath = "";
            if (File.Exists("C:\\Program Files\\SASHome\\SASFoundation\\9.4\\sas.exe")) { saspath = "C:\\Program Files\\SASHome\\SASFoundation\\9.4\\sas.exe"; }
            else if (File.Exists("C:\\Program Files (x86)\\SASHome\\SASFoundation\\9.4\\sas.exe")) { saspath = "C:\\Program Files (x86)\\SASHome\\SASFoundation\\9.4\\sas.exe"; }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "sas.exe|sas.sas";
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() == true)
                { saspath = openFileDialog.FileName; }
            }
            var listCopy = listView1.Items;
            if (listCopy.Count > 0 & saspath.Length > 1)
            {
                if ((LogCheck1.IsChecked == true & LogCheck2.IsChecked == false) || LogCheck1.IsChecked == false)
                {
                    for (int k = 0; k < listCopy.Count; k++)
                    {
                        ListItem thisItem = listCopy[k] as ListItem;
                        if (File.Exists(thisItem.Program.ToString()))
                        {
                            var sas = new ProcessStartInfo(@saspath);
                            string[] vs = thisItem.Program.ToString().Split('\\');
                            string wrkDir = "";
                            string sasIfle = vs[vs.Length - 1];
                            for (int i = 0; i < vs.Length - 1; i++)
                            {
                                wrkDir = wrkDir + vs[i] + "\\";
                            }
                            sas.WorkingDirectory = @wrkDir;
                            sas.Arguments = sasIfle;
                            var process = Process.Start(sas);
                            process.WaitForExit();
                            int sasExit = process.ExitCode;
                            if (sasExit < 3)
                            {
                                var lines = File.ReadAllLines(thisItem.Program.ToString().Replace(".sas", ".log"));
                                string[] excl = { "ONE OR MORE LIBRARIES SPECIFIED IN THE CONCATENATED LIBRARY SASHELP" , "DO NOT EXIST.  THESE LIBRARIES WERE REMOVED FROM THE CONCATENATION.",
                                    "UNABLE TO COPY SASUSER", "BASE PRODUCT PRODUCT", "EXPIRE WITHIN", "BASE SAS SOFTWARE", "EXPIRING SOON", "UPCOMING EXPIRATION",
                                    "SCHEDULED TO EXPIRE", "SETINIT TO OBTAIN MORE INFO" };
                                string[] issues = { "UNINITIALIZED", "NOTE: MERGE", "MORE THAN ONE DATA SET WITH REPEATS OF BY", "VALUES HAVE BEEN CONVERTED",
                                        "MISSING VALUES WERE GENERATED AS A RESULT", "INVALID DATA", "INVALID NUMERIC DATA", "AT LEAST ONE W.D FORMAT TOO SMALL",
                                        "ORDERING BY AN ITEM THAT DOESN'T APPEAR IN", "OUTSIDE THE AXIS RANGE", "RETURNING PREMATURELY", "UNKNOWN MONTH FOR",
                                        "QUERY DATA", "??", "QUESTIONABLE"};
                                foreach (var line in lines)
                                {
                                    if (!excl.Any(line.ToUpper().Contains))
                                    {
                                        if (issues.Any(line.ToUpper().Contains)) { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Issue" }; }
                                        if (line.Contains("WARNING:")) { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Warning" }; }
                                        if (line.Contains("ERROR:")) { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Error" }; }
                                    }
                                    else { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Ok" }; }
                                }
                            }
                            else if (sasExit == 6) { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "SAS issue" }; }
                            else { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Aborted" }; }
                        }
                        else { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Not exist" }; }
                    }
                }

                if (LogCheck1.IsChecked == true)
                {
                    for (int k = 0; k < listCopy.Count; k++)
                    {
                        ListItem thisItem = listCopy[k] as ListItem;
                        if (File.Exists(thisItem.Program.ToString().Replace(".sas", ".log")))
                        {
                            var lines = File.ReadAllLines(thisItem.Program.ToString().Replace(".sas", ".log"));
                            string[] excl = { "ONE OR MORE LIBRARIES SPECIFIED IN THE CONCATENATED LIBRARY SASHELP" , "DO NOT EXIST.  THESE LIBRARIES WERE REMOVED FROM THE CONCATENATION.",
                                    "UNABLE TO COPY SASUSER", "BASE PRODUCT PRODUCT", "EXPIRE WITHIN", "BASE SAS SOFTWARE", "EXPIRING SOON", "UPCOMING EXPIRATION",
                                    "SCHEDULED TO EXPIRE", "SETINIT TO OBTAIN MORE INFO" };
                            string[] issues = { "UNINITIALIZED", "NOTE: MERGE", "MORE THAN ONE DATA SET WITH REPEATS OF BY", "VALUES HAVE BEEN CONVERTED",
                                        "MISSING VALUES WERE GENERATED AS A RESULT", "INVALID DATA", "INVALID NUMERIC DATA", "AT LEAST ONE W.D FORMAT TOO SMALL",
                                        "ORDERING BY AN ITEM THAT DOESN'T APPEAR IN", "OUTSIDE THE AXIS RANGE", "RETURNING PREMATURELY", "UNKNOWN MONTH FOR",
                                        "QUERY DATA", "??", "QUESTIONABLE"};
                            foreach (var line in lines)
                            {
                                if (!excl.Any(line.ToUpper().Contains))
                                {
                                    if (issues.Any(line.ToUpper().Contains)) { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Issue" }; }
                                    if (line.Contains("WARNING:")) { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Warning" }; }
                                    if (line.Contains("ERROR:")) { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Error" }; }
                                }
                                else { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Ok" }; }
                            }
                        }
                        else { listView1.Items[k] = new ListItem() { Program = thisItem.Program, Status = "Not exist" }; }
                    }
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "HTML files (*.html)|*.html";
                    saveFileDialog.FileName = "logcheck.html";
                    saveFileDialog.RestoreDirectory = true;
                    string savefile = "";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        savefile = saveFileDialog.FileName;
                        if (File.Exists(@savefile))
                        {
                            File.Delete(@savefile);
                        }
                        using (StreamWriter writetext = new StreamWriter(savefile))
                        {
                            writetext.WriteLine("<html>");
                            writetext.WriteLine("<head>");
                            writetext.WriteLine("<style>");
                            writetext.WriteLine("table, th, td {");
                            writetext.WriteLine("  border: 1px solid black;");
                            writetext.WriteLine("  border-collapse: collapse;");
                            writetext.WriteLine("}");
                            writetext.WriteLine("th, td {");
                            writetext.WriteLine("  padding: 5px;");
                            writetext.WriteLine("}");
                            writetext.WriteLine("th {");
                            writetext.WriteLine("  text-align: left;");
                            writetext.WriteLine("}");
                            writetext.WriteLine("</style>");
                            writetext.WriteLine("</head>");
                            writetext.WriteLine("<body>");
                            writetext.WriteLine("<h1>Log check results</h1>");
                            writetext.WriteLine("<h2>" + DateTime.Now.ToString() + "</h2><br>");
                            writetext.WriteLine("<h3>Summary</h3>");
                            int nOk = 0;
                            int nIss = 0;
                            int nWar = 0;
                            int nErr = 0;
                            int nNex = 0;
                            int nSis = 0;
                            int nAbr = 0;
                            foreach (var eachItem in listView1.Items)
                            {
                                ListItem thisItem = eachItem as ListItem;
                                if (thisItem.Status.ToString() == "Ok") { nOk++; }
                                if (thisItem.Status.ToString() == "Issue") { nIss++; }
                                if (thisItem.Status.ToString() == "Warning") { nWar++; }
                                if (thisItem.Status.ToString() == "Error") { nErr++; }
                                if (thisItem.Status.ToString() == "Not exist") { nNex++; }
                                if (thisItem.Status.ToString() == "SAS issue") { nSis++; }
                                if (thisItem.Status.ToString() == "Aborted") { nAbr++; }
                            }
                            writetext.WriteLine("<table>");
                            if (nOk > 0) { writetext.WriteLine("<tr bgcolor=\"E1FFEA\"><td>Ok</td><td>" + nOk.ToString() + "</td></tr>"); }
                            if (nIss > 0) { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + nIss.ToString() + "</td></tr>"); }
                            if (nWar > 0) { writetext.WriteLine("<tr bgcolor=\"FFF7E9\"><td>Warning</td><td>" + nWar.ToString() + "</td></tr>"); }
                            if (nErr > 0) { writetext.WriteLine("<tr bgcolor=\"FFECE9\"><td>Error</td><td>" + nErr.ToString() + "</td></tr>"); }
                            if (nNex > 0) { writetext.WriteLine("<tr bgcolor=\"F1F1F1\"><td>Not exist</td><td>" + nNex.ToString() + "</td></tr>"); }
                            if (nSis > 0) { writetext.WriteLine("<tr bgcolor=\"F1F1F1\"><td>SAS issue</td><td>" + nSis.ToString() + "</td></tr>"); }
                            if (nAbr > 0) { writetext.WriteLine("<tr bgcolor=\"FFECE9\"><td>Aborted</td><td>" + nAbr.ToString() + "</td></tr>"); }
                            writetext.WriteLine("</table><br><br>");
                            writetext.WriteLine("<h3>List of the programs</h3>");
                            writetext.WriteLine("<table>");
                            writetext.WriteLine("<tr><th>Program</th><th>Status</th><th>Log Date and Time</th></tr>");
                            foreach (var eachItem in listView1.Items)
                            {
                                ListItem thisItem = eachItem as ListItem;
                                string[] vs = thisItem.Program.ToString().Split('\\');
                                string sasIfle = vs[vs.Length - 1];
                                string logdate = "";
                                if (File.Exists((thisItem.Program.ToString().Replace(".sas", ".log"))))
                                { logdate = File.GetLastWriteTime((thisItem.Program.ToString().Replace(".sas", ".log"))).ToString(); }
                                else { logdate = "Log missing"; }
                                if (thisItem.Status.ToString() == "Ok")
                                { writetext.WriteLine("<tr bgcolor=\"E1FFEA\"><td>" + sasIfle + "</td><td>" + thisItem.Status.ToString() + "</td><td>" + logdate + "</td></tr>"); }
                                if (thisItem.Status.ToString() == "Issue")
                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>" + sasIfle + "</td><td>" + thisItem.Status.ToString() + "</td><td>" + logdate + "</td></tr>"); }
                                if (thisItem.Status.ToString() == "Warning")
                                { writetext.WriteLine("<tr bgcolor=\"FFF7E9\"><td>" + sasIfle + "</td><td>" + thisItem.Status.ToString() + "</td><td>" + logdate + "</td></tr>"); }
                                if (thisItem.Status.ToString() == "Error")
                                { writetext.WriteLine("<tr bgcolor=\"FFECE9\"><td>" + sasIfle + "</td><td>" + thisItem.Status.ToString() + "</td><td>" + logdate + "</td></tr>"); }
                                if (thisItem.Status.ToString() == "Not exist")
                                { writetext.WriteLine("<tr bgcolor=\"F1F1F1\"><td>" + sasIfle + "</td><td>" + thisItem.Status.ToString() + "</td><td>" + logdate + "</td></tr>"); }
                                if (thisItem.Status.ToString() == "SAS issue")
                                { writetext.WriteLine("<tr bgcolor=\"F1F1F1\"><td>" + sasIfle + "</td><td>" + thisItem.Status.ToString() + "</td><td>" + logdate + "</td></tr>"); }
                                if (thisItem.Status.ToString() == "Aborted")
                                { writetext.WriteLine("<tr bgcolor=\"FFECE9\"><td>" + sasIfle + "</td><td>" + thisItem.Status.ToString() + "</td><td>" + logdate + "</td></tr>"); }
                            }
                            writetext.WriteLine("</table><br><br>");
                            if (nIss + nWar + nErr > 0)
                            {
                                writetext.WriteLine("<h3>Details</h3>");
                                foreach (var eachItem in listView1.Items)
                                {
                                    ListItem thisItem = eachItem as ListItem;
                                    string[] vs = thisItem.Program.ToString().Split('\\');
                                    string sasIfle = vs[vs.Length - 1];
                                    if (thisItem.Status.ToString() != "Ok")
                                    {
                                        string logdate = "";
                                        if (File.Exists((thisItem.Program.ToString().Replace(".sas", ".log"))))
                                        { logdate = File.GetCreationTime((thisItem.Program.ToString().Replace(".sas", ".log"))).ToString(); }
                                        else { logdate = "Missing"; }
                                        writetext.WriteLine("<h4>Program: " + sasIfle + "<br>");
                                        writetext.WriteLine("Log date and time: " + logdate + "</h4>");
                                        writetext.WriteLine("<table>");
                                        writetext.WriteLine("<tr><th>Issue</th><th>Line</th><th>Details</th></tr>");

                                        var lines = File.ReadAllLines(thisItem.Program.ToString().Replace(".sas", ".log"));
                                        string[] excl = { "ONE OR MORE LIBRARIES SPECIFIED IN THE CONCATENATED LIBRARY SASHELP" , "DO NOT EXIST.  THESE LIBRARIES WERE REMOVED FROM THE CONCATENATION.",
                                    "UNABLE TO COPY SASUSER", "BASE PRODUCT PRODUCT", "EXPIRE WITHIN", "BASE SAS SOFTWARE", "EXPIRING SOON", "UPCOMING EXPIRATION",
                                    "SCHEDULED TO EXPIRE", "SETINIT TO OBTAIN MORE INFO" };
                                        for (int j = 0; j < lines.Count() - 1; j++)
                                        {
                                            var k = j + 1;
                                            var line = lines[j];
                                            if (!excl.Any(line.ToUpper().Contains))
                                            {
                                                if (line.ToUpper().Contains("UNINITIALIZED"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Uninitialized</td></tr>"); }
                                                if (line.ToUpper().Contains("NOTE: MERGE"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Merge statement</td></tr>"); }
                                                if (line.ToUpper().Contains("MORE THAN ONE DATA SET WITH REPEATS OF BY"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Repeats of by statement</td></tr>"); }
                                                if (line.ToUpper().Contains("VALUES HAVE BEEN CONVERTED"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Values have been converted</td></tr>"); }
                                                if (line.ToUpper().Contains("MISSING VALUES WERE GENERATED AS A RESULT"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Missing values were generated</td></tr>"); }
                                                if (line.ToUpper().Contains("INVALID DATA"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Invalid data</td></tr>"); }
                                                if (line.ToUpper().Contains("INVALID NUMERIC DATA"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Invalid numeric</td></tr>"); }
                                                if (line.ToUpper().Contains("AT LEAST ONE W.D FORMAT TOO SMALL"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Format too small</td></tr>"); }
                                                if (line.ToUpper().Contains("ORDERING BY AN ITEM THAT DOESN'T APPEAR IN"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Absence of sorting variable</td></tr>"); }
                                                if (line.ToUpper().Contains("OUTSIDE THE AXIS RANGE"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Axis range issue</td></tr>"); }
                                                if (line.ToUpper().Contains("RETURNING PREMATURELY"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Program returning prematurely</td></tr>"); }
                                                if (line.ToUpper().Contains("UNKNOWN MONTH FOR"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Unknown month</td></tr>"); }
                                                if (line.ToUpper().Contains("QUERY DATA"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Query data issue</td></tr>"); }
                                                if (line.ToUpper().Contains("??"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Unacceptable format usage ??</td></tr>"); }
                                                if (line.ToUpper().Contains("??"))
                                                { writetext.WriteLine("<tr bgcolor=\"FCFFE9\"><td>Issue</td><td>" + k.ToString() + "</td><td>Questionable issue</td></tr>"); }
                                                if (line.Contains("WARNING:"))
                                                { writetext.WriteLine("<tr bgcolor=\"FFF7E9\"><td>Warning</td><td>" + k.ToString() + "</td><td>" + line.Replace("WARNING: ", "").Substring(0, (line.Replace("WARNING: ", "") + ".").IndexOf('.')) + "</td></tr>"); }
                                                if (line.Contains("ERROR:"))
                                                { writetext.WriteLine("<tr bgcolor=\"FFECE9\"><td>Error</td><td>" + k.ToString() + "</td><td>" + line.Replace("ERROR: ", "").Substring(0, (line.Replace("ERROR: ", "") + ".").IndexOf('.')) + "</td></tr>"); }
                                            }
                                        }

                                        writetext.WriteLine("</table>");
                                    }
                                }
                            }
                            writetext.WriteLine("</body>");
                            writetext.WriteLine("</html>");
                        }
                    }
                    if (savefile != "") { Process.Start(savefile); }
                }
            }
        }
        private void ClickStatus(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                ListItem thisItem = listView1.Items[i] as ListItem;
                listView1.Items[i] = new ListItem() { Program = thisItem.Program, Status = "" };
            }
        }
        private void ClickSaveFiles(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                string savefile = saveFileDialog.FileName;
                if (File.Exists(@savefile))
                {
                    File.Delete(@savefile);
                }
                using (StreamWriter writetext = new StreamWriter(savefile))
                {
                    foreach (var eachItem in listView1.Items)
                    {
                        ListItem thisItem = eachItem as ListItem;
                        writetext.WriteLine(thisItem.Program.ToString());
                    }
                }
            }
        }
    }
}
