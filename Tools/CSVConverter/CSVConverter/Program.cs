﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.IO.Pipes;
using System.Xml;

namespace CSVConverter {
    class Program {
        static int Main(string[] args) {

            var arguments = new Arguments();
            if (! arguments.Parse(args))
            {
                Console.WriteLine("Parsing input arguments failed: " +arguments.ErrorMessage );
                Console.WriteLine(Arguments.Usage);
                return -1;
                //throw new  Exception("Test runner execution failed");
            }
            var testCases = new TestCases();
            var testAssemblies=testCases.FindTestAssemblies(arguments.AssemblyDirectory);
            foreach (var testAssembly in testAssemblies)
            {
                testCases.ParseResultFile(testAssembly);
                string CsvName;
                CsvName = Path.GetFileNameWithoutExtension(testAssembly);
                FileInfo outputFile = new FileInfo(arguments.OutputDirectory.FullName+"\\"+CsvName+".csv");
                if (!outputFile.Directory.Exists)
                {
                    Directory.CreateDirectory(outputFile.DirectoryName);
                }
                testCases.WriteIntoCsv(outputFile.FullName);
                Console.WriteLine("The CSV FILE IS STORED IN     "+ arguments.OutputDirectory.FullName + "\\" + CsvName + ".csv");
            }
            return 1;
        }
    }
    enum TestStatus {
        Passed = 0,
        Failed  = 1,
        Aborted = 2
    }

    class TestCase {
        public string Name;
        public TestStatus Status;
        public string Duration;
        public string FailureMessage = string.Empty;
    }

    class TestCases {
        private List<TestCase> allTestCases = new List<TestCase>();
        private const string csvHeader = "Name,Status,Duration";
        private const string csvDelimiter = ",";
        public int PassedTests = 0;
        public int FailedTests = 0;

        public IReadOnlyCollection<TestCase> AllTestCases {
            get { return allTestCases; }
        }

        public IReadOnlyCollection<TestCase> UniqueTests
        {
            get { return allTestCases.Distinct().ToList(); }
        }

        public void AppendTest(TestCase testCase) {
            allTestCases.Add(testCase);
            if (testCase.Status == TestStatus.Passed) {
                PassedTests++;
            } else {
                // TODO: Tests which are not passed are considered as failed. this can be enahnced to segrated tests based a statuses further.
                FailedTests++;
            }
        }

        public void ParseResultFile(string resultFilePath) {
            // Note: This works only for the test results generated by: nunit.consolerunner v3.11.1
            // TODO: Should have a logic to find which type of input result file is provided.
            StringBuilder sb = new StringBuilder();
            //string delimit = ",";
            XDocument testResultDoc = XDocument.Load(resultFilePath);
            var testCasesActual = testResultDoc.Descendants("test-case");
            //var testCases = new TestCases();
            foreach (var testCaseActual in testCasesActual) {
                var testCase = new TestCase();
                testCase.Name = testCaseActual.Attribute("fullname").Value;
                var testResult = testCaseActual.Attribute("result").Value;
                testCase.Status = (TestStatus)Enum.Parse(typeof(TestStatus), testResult);
                if (testCase.Status != TestStatus.Passed)
                {
                    // TODO: Xpaths to be used.
                    testCase.FailureMessage =
                        testCaseActual.Element("failure").Element("message").Value;
                }
                testCase.Duration = testCaseActual.Attribute("duration").Value;

                this.AppendTest(testCase);
            }
        }

     public  List<string> FindTestAssemblies(DirectoryInfo searchDirectory)
        {
            List<string> testAssemblies = new List<string>();
            foreach (var file in searchDirectory.GetFiles(("*.xml")))
            {
                testAssemblies.Add(file.FullName);
            }

            return testAssemblies;
        }

        public void WriteIntoCsv(string filePath) {
            StreamWriter csvFileWriter = new StreamWriter(filePath);
            // Write header
            csvFileWriter.WriteLine(csvHeader);

            // TODO: Dont dump error message, which contains new line characters. Which has to be pre-processed.
            foreach (var testCase in AllTestCases) {
                var testCaseInfo = new StringBuilder();
                testCaseInfo.Append(testCase.Name + csvDelimiter);
                testCaseInfo.Append(testCase.Status + csvDelimiter);
                testCaseInfo.Append(testCase.Duration + csvDelimiter);
                csvFileWriter.WriteLine(testCaseInfo.ToString());
            }
            csvFileWriter.Close();
        }
    }
   
}
