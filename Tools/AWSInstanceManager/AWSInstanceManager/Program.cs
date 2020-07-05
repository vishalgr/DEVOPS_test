using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace AWSInstanceManager
{
    class Program
    {
        public static void Main(string[] args)
        {
            var arguments = new Arguments();
            string AccessKey = ConfigurationManager.AppSettings.Get("AccessKey");
            string SecretKey = ConfigurationManager.AppSettings.Get("SecretKey");
            string amiId = ConfigurationManager.AppSettings.Get("amiId");
            string securityGroupId = ConfigurationManager.AppSettings.Get("securityGroupId");
            string keyPairName = ConfigurationManager.AppSettings.Get("keyPairName");
            string instanceSize = ConfigurationManager.AppSettings.Get("instanceSize");
            Amazon.Util.ProfileManager.RegisterProfile("default", AccessKey, SecretKey);
            var ec2Client = new AmazonEC2Client(RegionEndpoint.USWest2);
            string subnetId = ConfigurationManager.AppSettings.Get("subnetId");
            InstanceNetworkInterfaceSpecification networkSpecification = new InstanceNetworkInterfaceSpecification()
            {
                DeviceIndex = 0,
                SubnetId = subnetId,
                Groups = new List<string>() { securityGroupId },
                AssociatePublicIpAddress = true
            };
            if (!arguments.Parse(args))
            {
                Console.WriteLine("Parsing input arguments failed: " + arguments.ErrorMessage);
                Console.WriteLine(Arguments.Usage);
            }
            //calling CreateInstance using amiId
            if (arguments.OptionVal == 1)
            {
                CreateInstance(ec2Client, arguments.AMIid, instanceSize, arguments.MinCount, arguments.MaxCount, keyPairName, networkSpecification);
            }
            //Calling Terminate a aprticular instance using instance ID
            if (arguments.OptionVal == 2)
            {
                TerminateInstance(ec2Client, arguments.InstanceId);
            }
            //calling TerminateAllInstance
            if (arguments.OptionVal == 3)
            {
                //TerminateallInstance(ec2Client);
            }
        }
        //Function tocreate an Instance
        public static void CreateInstance(AmazonEC2Client ec2Client, string amiId, string instanceSize, int MinCount, int MaxCount, string keyPairName, InstanceNetworkInterfaceSpecification networkSpecification)
        {
            var arguments = new Arguments();
            List<InstanceNetworkInterfaceSpecification> networkSpecifications = new List<InstanceNetworkInterfaceSpecification>() { networkSpecification };
            RunInstancesRequest launchRequest = new RunInstancesRequest()
            {
                ImageId = amiId,
                InstanceType = instanceSize,
                MinCount = arguments.MinCount,
                MaxCount = arguments.MaxCount,
                KeyName = keyPairName,
                NetworkInterfaces = networkSpecifications
            };
            var data = ec2Client.RunInstancesAsync(launchRequest);
            while (!data.IsCompleted)
            {
                Console.WriteLine("Still contnues to run");
                Thread.Sleep(10000);
            }
            var result = data.Result;
            Console.WriteLine(result.ToString());
        }
        //Function to Terminate all the Instances
        public static void TerminateallInstance(AmazonEC2Client ec2Client)
        {
            DescribeInstancesResponse result = ec2Client.DescribeInstances();

            foreach (Reservation reservation in result.Reservations)
            {
                foreach (Instance instance in reservation.Instances)
                {
                    Console.WriteLine(instance.InstanceId);
                    TerminateInstance(ec2Client, instance.InstanceId);
                }
            }
            Thread.Sleep(10000);
        }
        //Function to terminate Instance based on Instance ID
        public static void TerminateInstance(AmazonEC2Client ec2Client, string instanceId)
        {
            var request = new TerminateInstancesRequest();
            request.InstanceIds = new List<string>() { instanceId };

            try
            {
                var response = ec2Client.TerminateInstances(request);
                foreach (InstanceStateChange item in response.TerminatingInstances)
                {
                    Console.WriteLine("Terminated instance: " + item.InstanceId);
                    Console.WriteLine("Instance state: " + item.CurrentState.Name);
                }
            }
            catch (AmazonEC2Exception ex)
            {
                // Check the ErrorCode to see if the instance does not exist.
                if ("InvalidInstanceID.NotFound" == ex.ErrorCode)
                {
                    Console.WriteLine("Instance {0} does not exist.", instanceId);
                }
                else
                {
                    // The exception was thrown for another reason, so re-throw the exception.
                    throw;
                }
            }
        }
    }
}

