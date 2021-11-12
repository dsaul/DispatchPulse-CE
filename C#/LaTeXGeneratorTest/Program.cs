using Databases.Records.CRM;
using LaTeXGenerators;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace LaTeXGeneratorTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("LaTeX Generator Test");




			string billingConnectionString = Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.KBillingDatabaseName);
			if (string.IsNullOrWhiteSpace(billingConnectionString)) {
				Console.WriteLine("Couldn't get connection information for the billing system.");
				return;
			}


			using NpgsqlConnection billingConnection = new NpgsqlConnection(billingConnectionString);
			if (null == billingConnection) {
				Console.Write("Couldn't open a connection to the billing system. #1");
				return;
			}

			billingConnection.Open();
			if (billingConnection.State != System.Data.ConnectionState.Open) {
				Console.WriteLine("Couldn't open a connection to the billing system. #2");
				return;
			}



			string dpDBConnectionString = Databases.Konstants.DatabaseConnectionStringForDB("zclient_dp");
			if (string.IsNullOrWhiteSpace(dpDBConnectionString)) {
				Console.WriteLine("Cannot get database connection string.");
				return;
			}

			using NpgsqlConnection dpDBConnection = new NpgsqlConnection(dpDBConnectionString);
			if (null == billingConnection) {
				Console.WriteLine("null == dbConnection");
				return;
			}
			dpDBConnection.Open();
			if (dpDBConnection.State != System.Data.ConnectionState.Open) {
				Console.WriteLine("dbConnection.State != System.Data.dbConnection.Open");
				return;
			}

#if false
			var res = Assignments.All(dpDBConnection);
			if (res.Count == 0) {
				Console.WriteLine("res.Count == 0");
				return;
			}

			var first = res.FirstOrDefault();
			List<Assignments> list = new List<Assignments>();
			list.Add(first.Value);

			Console.WriteLine("Assignments Output:");
			Console.WriteLine("=================================================================");
			var str = LaTeXAssignments.Generate(billingConnection, dpDBConnection, true, true, list);
			Console.WriteLine(str);
#endif

#if false
			var res = Companies.All(dpDBConnection);
			if (res.Count == 0) {
				Console.WriteLine("res.Count == 0");
				return;
			}

			Console.WriteLine("Companies Output:");
			Console.WriteLine("=================================================================");
			var str = LaTeXCompanies.Generate(billingConnection, dpDBConnection, true, true, res.Values.ToList());
			Console.WriteLine(str);
#endif

#if false
			var res = Contacts.All(dpDBConnection);
			if (res.Count == 0) {
				Console.WriteLine("res.Count == 0");
				return;
			}

			Console.WriteLine("Contacts Output:");
			Console.WriteLine("=================================================================");
			var str = LaTeXContacts.Generate(billingConnection, dpDBConnection, true, true, res.Values.ToList());
			Console.WriteLine(str);
#endif

#if false
			var res = Labour.ForAgentId(dpDBConnection, Guid.Parse("7ccd0eee-a9df-455f-a4a7-d978577d0067"));
			if (res.Count == 0) {
				Console.WriteLine("res.Count == 0");
				return;
			}

			Console.WriteLine("Labour Output:");
			Console.WriteLine("=================================================================");
			var str = LaTeXLabour.Generate(billingConnection, dpDBConnection, true, true, res.Values.ToList()).Result;

			string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

			File.WriteAllText(Path.Join(desktopPath, "debugtex.tex"), str);
			Console.WriteLine("done");
#endif

#if false
			var res = Materials.All(dpDBConnection);
			if (res.Count == 0) {
				Console.WriteLine("res.Count == 0");
				return;
			}

			Console.WriteLine("Materials Output:");
			Console.WriteLine("=================================================================");
			var str = LaTeXMaterials.Generate(billingConnection, dpDBConnection, true, true, res.Values.ToList());
			Console.WriteLine(str.Result);
#endif

			var res = Projects.ForId(dpDBConnection, Guid.Parse("351502b7-17af-4990-9ef3-e5f3dc01f8bd"));
			if (res.Count == 0) {
				Console.WriteLine("res.Count == 0");
				return;
			}

			Console.WriteLine("Projects Output:");
			Console.WriteLine("=================================================================");
			var str = LaTeXProjects.Generate(
				billingConnection, 
				dpDBConnection,
				true,
				true,
				res.Values.ToList(),
				true,
				true,
				true,
				true,
				true,
				true
				);
			Console.WriteLine(str.Result);





		}
	}
}
