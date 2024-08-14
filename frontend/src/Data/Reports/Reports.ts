import { RPCGetPDFLaTeXTask } from "@/Data/Reports/RPCGetPDFLaTeXTask";
import { RPCRunReportAssignments } from "@/Data/Reports/RPCRunReportAssignments";
import { RPCRunReportCompanies } from "@/Data/Reports/RPCRunReportCompanies";
import { RPCRunReportContacts } from "@/Data/Reports/RPCRunReportContacts";
import { RPCRunReportLabour } from "@/Data/Reports/RPCRunReportLabour";
import { RPCRunReportMaterials } from "@/Data/Reports/RPCRunReportMaterials";
import { RPCRunReportProjects } from "@/Data/Reports/RPCRunReportProjects";
import { RPCMethod } from "@/RPC/RPCMethod";
import { RPCRunReportOnCallResponder30Day } from "@/Data/Reports/RPCRunReportOnCallResponder30Day";

export class Reports {
	// RPC Methods
	public static GetPDFLaTeXTask = RPCMethod.Register<RPCGetPDFLaTeXTask>(
		new RPCGetPDFLaTeXTask()
	);
	public static RunReportAssignments = RPCMethod.Register<
		RPCRunReportAssignments
	>(new RPCRunReportAssignments());
	public static RunReportCompanies = RPCMethod.Register<
		RPCRunReportCompanies
	>(new RPCRunReportCompanies());
	public static RunReportContacts = RPCMethod.Register<RPCRunReportContacts>(
		new RPCRunReportContacts()
	);
	public static RunReportLabour = RPCMethod.Register<RPCRunReportLabour>(
		new RPCRunReportLabour()
	);
	public static RunReportMaterials = RPCMethod.Register<
		RPCRunReportMaterials
	>(new RPCRunReportMaterials());
	public static RunReportProjects = RPCMethod.Register<RPCRunReportProjects>(
		new RPCRunReportProjects()
	);
	public static RunReportOnCallResponder30Day = RPCMethod.Register<
		RPCRunReportOnCallResponder30Day
	>(new RPCRunReportOnCallResponder30Day());
}

(window as any).DEBUG_Reports = Reports;
