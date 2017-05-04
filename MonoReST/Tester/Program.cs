using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Emc.Documentum.Rest.DataModel;
using Emc.Documentum.Rest.Net;
using Emc.Documentum.Rest.Http.Utility;
using System.IO;
using System.Collections.Specialized;
using System.Windows.Forms;
using Emc.Documentum.Rest.DataModel.D2;

namespace Emc.Documentum.Rest.Test
{
    class Program
    {
        private static string RestHomeUri = "#EXAMPLE: http://apps.company.com/dctm-rest/services):";
        private static string username = "#EXAMPLE: dmadmin";
        private static string password = "#EXAMPLE: MyS^p3Cc3P@55W0rd!";
        private static string repositoryName = "#EXAMPLE: CompanyDocbase";
        private static bool printResult = false;
        private static bool hasConfigInitialized = false;
        private static RestController client;

        [STAThread]
        static void Main(string[] args)
        {

            Console.ForegroundColor = ConsoleColor.White;
            Console.WindowWidth = 120;
            Console.WindowHeight = 40;
            Console.BufferWidth = 120;
            Console.BufferHeight = 1000;

            NameValueCollection config = getDefaultConfiguration();
            bool useDefault = config != null && Boolean.Parse(config["useDefaults"]);
            SetupTestData(useDefault);

            Arguments cmd = PrintMenu();
            while (!cmd.Exit())
            {
                string next = cmd.Next();
                try
                {
                    switch (next.ToLower())
                    {
                        case "dql":
                            int itemsPerPage = cmd.IsNextInt() ? cmd.NextInt() : 10;
                            bool pause = cmd.IsNextBool() ? cmd.NextBool() : false;
                            string dql = cmd.ReadToEnd();
                            DqlQueryTest.Run(client, RestHomeUri, dql, itemsPerPage, pause, repositoryName, printResult);
                            break;
                        case "search":
                            itemsPerPage = cmd.IsNextInt() ? cmd.NextInt() : 10;
                            pause = cmd.IsNextBool() ? cmd.NextBool() : false;
                            string terms = cmd.ReadToEnd();
                            SearchTest.Run(client, RestHomeUri, terms, itemsPerPage, pause, repositoryName, printResult);
                            break;
                        case "content":
                            int numDocs = cmd.IsNextInt() ? cmd.NextInt() : 10;
                            int threadCount = cmd.IsNextInt() ? cmd.NextInt() : 1;
                            pause = cmd.IsNextBool() ? cmd.NextBool() : false;
                            testUseCase(numDocs, threadCount, pause);
                            break;
                        case "format":
                            itemsPerPage = cmd.IsNextInt() ? cmd.NextInt() : 10;
                            pause = cmd.IsNextBool() ? cmd.NextBool() : false;
                            string filter = cmd.ReadToEnd();
                            FormatTest.Run(client, RestHomeUri, filter, itemsPerPage, pause, repositoryName, printResult);
                            break;
                        case "type":
                            itemsPerPage = cmd.IsNextInt() ? cmd.NextInt() : 10;
                            pause = cmd.IsNextBool() ? cmd.NextBool() : false;
                            string parent = cmd.ReadToEnd();
                            TypeTest.Run(client, RestHomeUri, parent, itemsPerPage, pause, repositoryName, printResult);
                            break;
                        case "user":
                            itemsPerPage = cmd.IsNextInt() ? cmd.NextInt() : 10;
                            pause = cmd.IsNextBool() ? cmd.NextBool() : false;
                            filter = cmd.Next();
                            string userName = cmd.ReadToEnd();
                            UserTest.Run(client, RestHomeUri, filter, itemsPerPage, pause, repositoryName, printResult, userName);
                            break;
                        case "group":
                            itemsPerPage = cmd.IsNextInt() ? cmd.NextInt() : 10;
                            pause = cmd.IsNextBool() ? cmd.NextBool() : false;
                            filter = cmd.Next();
                            string groupName = cmd.ReadToEnd();
                            GroupTest.Run(client, RestHomeUri, filter, itemsPerPage, pause, repositoryName, printResult, groupName);
                            break;
                        case "group-member":                           
                            string testGroup = cmd.Next();
                            string testSubGroup = cmd.Next();
                            string testUser = cmd.ReadToEnd();
                            GroupMemberTest.Run(client, RestHomeUri, repositoryName, printResult, testGroup,testSubGroup,testUser);
                            break;
                        case "batch":
                            BatchTest.Run(client, RestHomeUri, repositoryName);
                            break;
                        case "d2tests":
                            numDocs = cmd.IsNextInt() ? cmd.NextInt() : 10;
                            threadCount = cmd.IsNextInt() ? cmd.NextInt() : 1;
                            d2Tests(numDocs, threadCount);
                            break;
                        case "cls":
                            Console.Clear();
                            break;
                        case "reconfig":
                            SetupTestData(false);
                            break;
                        default:
                            Console.WriteLine("Nothing entered");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Got exception in test: " + e.Message + "\r\n" + e.StackTrace);
                }

                if (!next.Equals("cls"))
                {
                    Console.WriteLine("Press any key to continue...\r\n");
                    Console.ReadKey();
                }
                cmd = PrintMenu();
            }
        }

        private static void d2Tests(int numDocs, int threadCount)
        {
            long start = DateTime.Now.Ticks;
            if (threadCount >= 1)
            {
                for (int i = 0; i < threadCount; i++)
                {
                    UseCaseTestsD2 aTest = new UseCaseTestsD2(new RestController(username, password),
                            RestHomeUri, repositoryName, threadCount > 1 ? true : false, "/Temp/Test-" + DateTime.Now.Ticks, i, numDocs);
                    ThreadStart job = new ThreadStart(aTest.Start);
                    new Thread(job).Start();
                }
                Console.WriteLine("\n\n " + numDocs + " documents will be imported and randomly assigned to " + threadCount + " cases, 5 requests for each of "
                + threadCount + " threads");
            }
            else
            {
                UseCaseTestsD2 aTest = new UseCaseTestsD2(new RestController(username, password),
                        RestHomeUri, repositoryName, threadCount > 1 ? true : false, "/Temp/Test-" + DateTime.Now.Ticks, 1, numDocs);
                aTest.Start();
            }

        }

        private static void testUseCase(int numDocs, int threadCount, bool pauseBetweenOperations)
        {
            bool printResult = threadCount > 1 ? true : false;
            long start = DateTime.Now.Ticks;
            if (threadCount > 1)
            {
                for (int i = 0; i < threadCount; i++)
                {
                    UseCaseTests aTest = new UseCaseTests(new RestController(username, password),
                            RestHomeUri, repositoryName, printResult, false, "/Temp/Test-" + DateTime.Now.Ticks, i, numDocs);
                    ThreadStart job = new ThreadStart(aTest.Start);
                    new Thread(job).Start();
                }
                Console.WriteLine("\n\n " + numDocs + " documents will be imported and randomly assigned to " + threadCount + " cases, 5 requests for each of "
                + threadCount + " threads");
            }
            else
            {
                UseCaseTests aTest = new UseCaseTests(new RestController(username, password),
                        RestHomeUri, repositoryName, printResult, pauseBetweenOperations, "/Temp/Test-" + DateTime.Now.Ticks, 1, numDocs);
                aTest.Start();
            }
        }

        private static string getPrimaryCommand(String line)
        {
            String test = "";
            if (!String.IsNullOrWhiteSpace(line))
            {
                test = line.IndexOf(" ") > 0 ? line.Substring(0, line.IndexOf(" ")) : line;
            }
            return test;
        }

        private static int getInputNumber(string title, int value)
        {

            string input = "";
            try
            {
                Console.WriteLine("\t" + title + " [" + value + "]");
                Console.WriteLine();
                input = getLineOfInput("", "");
                if (input != null && !input.Trim().Equals(""))
                {
                    value = int.Parse(input);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Input: " + input + " cannot be parsed to an integer, using default value of " + value, e);
            }
            return value;
        }

        public static Arguments PrintMenu()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("\treconfig \n\t\t- prompt for re-entering configuration information.");
            Console.WriteLine("\tdql [<itemsPerPage> <pauseBetweenPages>] <dqlQuery>"
                            + "\n\t\t- Runs a DQL query and prints the results."
                            + "\n\t\t    <itemsPerPage>      - Number of items per DQL query result page"
                            + "\n\t\t    <pauseBetweenPages> - Indicating whether to pause beteen DQL query result pagination"
                            + "\n\t\t    <dqlQuery>          - DQL query expression; mandatory"
                            + "\n\t\t- Example:"
                            + "\n\t\t    dql 10 false select * from dm_cabinet");
            Console.WriteLine("\tcontent [<numDocs> <numThreads> <pause>]"
                            + "\n\t\t- Runs the end to end content CRUD tests with optional threads and number of documents. "
                            + "\n\t\t    <numDocs>           - Number of documents to import"
                            + "\n\t\t    <numThreads>        - Number of REST client threads"
                            + "\n\t\t    <pause>             - Flag indicating whether to pause beteen operations; single thread only"
                            + "\n\t\t- Example:"
                            + "\n\t\t    content 10 1 true");
            Console.WriteLine("\tsearch [<itemsPerPage> <pauseBetweenPages>] <terms>"
                            + "\n\t\t- Runs the full-text search and prints the results. "
                            + "\n\t\t    <itemsPerPage>      - Number of items per search result page"
                            + "\n\t\t    <pauseBetweenPages> - Indicating whether to pause beteen search result pagination"
                            + "\n\t\t    <terms>             - Terms to search in simple search language"
                            + "\n\t\t- Example:"
                            + "\n\t\t    search 10 true rest and documentum");
            Console.WriteLine("\tformat [<itemsPerPage> <pauseBetweenPages> <filter>]"
                            + "\n\t\t- Gets formats and prints the results. "
                            + "\n\t\t    <itemsPerPage>      - Number of items per formats page"
                            + "\n\t\t    <pauseBetweenPages> - Indicating whether to pause beteen formats pagination"
                            + "\n\t\t    <filter>            - Filter expression to filter result; empty to return all formats"
                            + "\n\t\t- Example:"
                            + "\n\t\t    format 10 true starts-with(mime_type, 'text')");
            Console.WriteLine("\ttype [<itemsPerPage> <pauseBetweenPages> <parentType>]"
                            + "\n\t\t- Gets types and prints the results. "
                            + "\n\t\t    <itemsPerPage>      - Number of items per types page"
                            + "\n\t\t    <pauseBetweenPages> - Indicating whether to pause beteen types pagination"
                            + "\n\t\t    <parentType>        - Get sub types for the specified parent; emtpy to return all types"
                            + "\n\t\t- Example:"
                            + "\n\t\t    format 10 true starts-with(mime_type, 'text')");
            Console.WriteLine("\tbatch"
                            + "\n\t\t- Execute a batch service. ");
            Console.WriteLine("\tuser [<itemsPerPage> <pauseBetweenPages> <filter> <userNameToCreatedOrRetrieved>]"
                            + "\n\t\t- Gets users, CRUD user and prints the results. "
                            + "\n\t\t    <itemsPerPage>       - Number of items per users page"
                            + "\n\t\t    <pauseBetweenPages>  - Indicating whether to pause beteen users pagination"
                            + "\n\t\t    <filter>             - Filter expression to filter result; 'null' to return all users"
                            + "\n\t\t    <userName>  - Specify the value of user_name that you want to create with or retrieve"
                            + "\n\t\t-  Example:"
                            + "\n\t\t   user 10 true starts-with(user_name,'dm') net_sample_test_user");
            Console.WriteLine("\tgroup [<itemsPerPage> <pauseBetweenPages> <filter> <groupNameToCreatedOrRetrieved>]"
                          + "\n\t\t- Gets groups, CRUD group and prints the results. "
                          + "\n\t\t    <itemsPerPage>       - Number of items per groups page"
                          + "\n\t\t    <pauseBetweenPages>  - Indicating whether to pause beteen groups pagination"
                          + "\n\t\t    <filter>             - Filter expression to filter result; 'null' to return all groups"
                          + "\n\t\t    <groupName>  - Specify the value of group_name that you want to create with or retrieve"
                          + "\n\t\t-  Example:"
                          + "\n\t\t   group 10 true starts-with(group_name,'ne') net_sample_test_group");
            Console.WriteLine("\tgroup-member [<parentGroupName> <subGroupName> <userName>]"
                         + "\n\t\t- Add user/sub-group to parent group, remove user/sub-group from the parent group. "
                         + "\n\t\t    <parentGroupName>  - Specify the value of group_name that you want to add user/sub-group to and remove user/sub-group from"
                         + "\n\t\t    <subGroupName>     - Specify the value of group_name that you want to be added to or removed from the parent group"
                         + "\n\t\t    <userName>  - Specify the value of user_name that you want to be added to or removed from the parent group"                       
                         + "\n\t\t-  Example:"
                         + "\n\t\t   group-member net_sample_test_group net_sample_test_sub_group net_sample_test_user");            
            Console.WriteLine("\tcls \n\t\t- Clear the console");
            Console.WriteLine("\texit \n\t\t- Exit the Test");
            Console.Write("\r\n\nCommand > ");

            return new Arguments(getLineOfInput("", "").Trim());
        }

        private static string getLineOfInput(String prompt, String defaultValue)
        {
            string line = null;
            bool isInputValid = false;

            while (!isInputValid)
            {
                if (!String.IsNullOrEmpty(prompt)) Console.WriteLine(prompt);
                Console.ForegroundColor = ConsoleColor.Yellow;
                line = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                if (String.IsNullOrEmpty(line) && defaultValue.StartsWith("#EXAMPLE: ") || line.StartsWith("#EXAMPLE: "))
                {
                    Console.WriteLine("\nInvalid input, value should not start with \"#EXAMPLE: \"");
                }
                else
                {
                    isInputValid = true;
                }

            }

            return line;
        }

        private static String getLineOfHiddenInput(String prompt, String defaultValue)
        {
            if (!String.IsNullOrEmpty(prompt)) Console.WriteLine(prompt);
            ConsoleKeyInfo inf;
            StringBuilder input = new StringBuilder();
            inf = Console.ReadKey(true);
            while (inf.Key != ConsoleKey.Enter)
            {
                input.Append(inf.KeyChar);
                inf = Console.ReadKey(true);
            }
            Console.WriteLine();
            return input.ToString();

        }

        private static void SetupTestData(bool useDefaults)
        {

            NameValueCollection restConfig = getDefaultConfiguration();
            if (restConfig != null && !hasConfigInitialized)
            {
                RestHomeUri = restConfig["defaultRestHomeUri"];
                username = restConfig["defaultUsername"];
                password = restConfig["defaultPassword"];
                repositoryName = restConfig["defaultRepositoryName"];
                printResult = Boolean.Parse(restConfig["defaultPrintResult"].ToString());
                validateConfig();
                if (hasConfigInitialized)
                {
                    Console.Write("Used default settings. ");
                    printConfiguration();
                    setupClient();
                    return;
                }
            }
            hasConfigInitialized = false;
            while (!hasConfigInitialized)
            {
                if (useDefaults && hasConfigInitialized)
                {
                    Console.Write("Configuration completed with default settings. ");
                    printConfiguration();
                }
                else
                {
                    readSetupParameters(RestHomeUri, username, password, repositoryName, printResult.ToString());
                    hasConfigInitialized = true;
                    Console.Write("Re-configuration completed. ");
                    printConfiguration();
                    validateConfig();
                }
            }

            setupClient();

        }

        private static void setupClient()
        {
            client = String.IsNullOrEmpty(password) ? new RestController(null, null) : new RestController(username, password);
            // alternatively, you can choose .net default data contract serializer: new DefaultDataContractJsonSerializer();
            JsonDotnetJsonSerializer serializer = new JsonDotnetJsonSerializer();
            serializer.PrintStreamBeforeDeserialize = true;
            client.JsonSerializer = serializer;
        }

        private static void validateConfig()
        {
            if (!(
                RestHomeUri.StartsWith("#EXAMPLE: ")
                && username.StartsWith("#EXAMPLE: ")
                && password.StartsWith("#EXAMPLE: ") //Ok, so your password can't start with #EXAMPLE: but I will take those odds
                && repositoryName.StartsWith("#EXAMPLE: ")
                )
             )
            {
                hasConfigInitialized = true;
            }
            else
            {
                hasConfigInitialized = false;
            }
        }

        private static NameValueCollection getDefaultConfiguration()
        {
            try
            {
                return ConfigurationManager.GetSection("restconfig") as NameValueCollection;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Configuration could  not load. If you are running under Visual Studio, ensure:\n" +
                    "\n\"<section name=\"Restconfig\" type=\"System.Configuration.NameValueSectionHandler\"/> is used. " +
                    "\nIf running under Mono, ensure: " +
                    "\n<section name=\"Restconfig\" type=\"System.Configuration.NameValueSectionHandler,System\"/> is used");
                return null;
            }
        }

        private static void printConfiguration()
        {
            Console.WriteLine("Please review your settings below:\r\n");
            Console.WriteLine("\tHome document URL:\t [" + RestHomeUri + "]");
            Console.WriteLine("\tUser login name:\t [" + username + "]");
            Console.WriteLine("\tRepository name:\t [" + repositoryName + "]");
            Console.WriteLine("\tPrint the result?\t [" + printResult + "]\r\n");
        }

        public static void readSetupParameters(string defaultRestHomeUri, string defaultUsername, string defaultPassword,
            string defaultRepositoryName, string defaultPrintResult)
        {
            RestHomeUri = getLineOfInput("Set the home document URL: [" + defaultRestHomeUri + "]", defaultRestHomeUri);
            if (String.IsNullOrEmpty(RestHomeUri)) RestHomeUri = defaultRestHomeUri;

            username = getLineOfInput("Set the username: [" + defaultUsername + "]", defaultUsername);
            if (String.IsNullOrEmpty(username)) username = defaultUsername;


            password = getLineOfHiddenInput("Set the user password: [**********]", defaultPassword);
            if (String.IsNullOrEmpty(password)) password = defaultPassword;

            repositoryName = getLineOfInput("Set the repository name: [" + defaultRepositoryName + "]", defaultRepositoryName);
            if (String.IsNullOrEmpty(repositoryName)) repositoryName = defaultRepositoryName;

            string input = getLineOfInput("Whether to print the result? [" + defaultPrintResult + "]", defaultPrintResult);
            printResult = String.IsNullOrEmpty(input) ? Boolean.Parse(defaultPrintResult) : Boolean.Parse(input);
        }
    }
}

