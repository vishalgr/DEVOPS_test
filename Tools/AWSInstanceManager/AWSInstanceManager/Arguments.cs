using System;
using System.Text;

namespace AWSInstanceManager
{
    public class Arguments
    {
        string amiId, instanceId;
        int mincount, maxcount, count = 0;
        private readonly StringBuilder errorMessage = new StringBuilder();
        private static readonly string currentAssembly = typeof(Arguments).Assembly.GetName().Name;
        public bool Parse(string[] commandLinArgs)
        {
            if (commandLinArgs.Length < 1)
            {
                errorMessage.AppendLine("Input arguments cannot be null");
                return false;
            }
            var isParseSuccess = true;
            for (int i = 0; i < commandLinArgs.Length; i++)
            {
                switch (commandLinArgs[i].ToUpper().Trim())
                {
                    case "--CREATEINSTANCE":
                        for (int j = 1; j < commandLinArgs.Length; j++)
                        {
                            switch (commandLinArgs[j].ToUpper().Trim())
                            {
                                case "--AMIID":
                                    amiId = commandLinArgs[++j].Trim();
                                    break;
                                case "--MINCOUNT":
                                    mincount = Int32.Parse(commandLinArgs[++j].Trim());
                                    break;
                                case "--MAXCOUNT":
                                    maxcount = Int32.Parse(commandLinArgs[++j].Trim());
                                    break;
                            }
                        }
                        count = 1;
                        return isParseSuccess;

                    case "--TERMINATEINSTANCE":
                        for (int j = 1; j < commandLinArgs.Length; j++)
                        {
                            switch (commandLinArgs[j].ToUpper().Trim())
                            {
                                case "--INSTANCEID":
                                    instanceId = commandLinArgs[++j].Trim();
                                    break;
                            }
                        }
                        count = 2;
                        return isParseSuccess;

                    case "--TERMINATEALLINSTANCES":
                        count = 3;
                        return isParseSuccess;

                    default:
                        errorMessage.AppendLine("Invalid input argument: " + commandLinArgs[i]);
                        isParseSuccess = false;
                        break;
                }
            }
            return isParseSuccess;
        }
        public static string Usage
        {
            get
            {
                var newLine = Environment.NewLine;
                var help = new StringBuilder("Usage information:");
                help.AppendLine(
                    currentAssembly +
                    " There are 3 options available here"
                );
                help.AppendLine("1)CREATING AN INSTANCE 2)TERMINATING AN INSTANCE  3)TERMINATING ALL INSATNCES");
                help.AppendLine("1)Creating an Instance");
                help.AppendLine("Syntax:" + currentAssembly + " --CreateInstance --AMIid <amiId> --MinCount <num> --MaxCount <num>");
                help.AppendLine("AMIid: AMIid of the insatnce");
                help.AppendLine("MaxCount: Maximum No. Instances To be Created");
                help.AppendLine("EXAMPLE:" + currentAssembly + " --CreateInstance --AMIid <amiId> --MinCount <num> --MaxCount <num>");
                help.AppendLine("");
                help.AppendLine("2)Terminating an Instance");
                help.AppendLine("Syntax:" + currentAssembly + " --TerminateInstance --InstanceId <instanceId> ");
                help.AppendLine("Instance ID: ID of an AWS insatnce");
                help.AppendLine("Example:" + currentAssembly + " --TerminateInstance --InstanceId <instanceId> ");
                help.AppendLine("");
                help.AppendLine("3)Terminating all Instances");
                help.AppendLine("Syntax:" + currentAssembly + " --TerminateAllInstances");
                return help.ToString();
            }
        }
        public string AMIid
        {
            get
            {
                return amiId;
            }
        }
        public string InstanceId
        {
            get
            {
                return instanceId;
            }
        }
        public int MinCount
        {
            get
            {
                return mincount;
            }
        }
        public int MaxCount
        {
            get
            {
                return maxcount;
            }
        }
        public int OptionVal
        {
            get
            {
                return count;
            }
        }
        public string ErrorMessage
        {
            get { return errorMessage.ToString(); }
        }
    }
}
