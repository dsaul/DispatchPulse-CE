import { Vue } from "vue-property-decorator";
import Router from "vue-router";
import Dashboard from "@/Components/Views/Dashboard.vue";
import DashboardAgent from "@/Components/Views/DashboardAgent.vue";
import DashboardBilling from "@/Components/Views/DashboardBilling.vue";
import DashboardDispatch from "@/Components/Views/DashboardDispatch.vue";
import DashboardManagement from "@/Components/Views/DashboardManagement.vue";
import DashboardOnCall from "@/Components/Views/DashboardOnCall.vue";
import ContactDisplay from "@/Components/Views/ContactDisplay.vue";
import CompanyDisplay from "@/Components/Views/CompanyDisplay.vue";
import CompaniesIndex from "@/Components/Views/CompaniesIndex.vue";
import ContactsIndex from "@/Components/Views/ContactsIndex.vue";

import ProjectsIndex from "@/Components/Views/ProjectsIndex.vue";
import ProjectsDisplay from "@/Components/Views/ProjectsDisplay.vue";
import AgentsIndex from "@/Components/Views/AgentsIndex.vue";
import AssignmentsIndex from "@/Components/Views/AssignmentsIndex.vue";
import MaterialsIndex from "@/Components/Views/MaterialsIndex.vue";
import ProductsDefinitions from "@/Components/Views/ProductsDefinitions.vue";
import LabourNonBillableDefinitions from "@/Components/Views/LabourNonBillableDefinitions.vue";
import LabourHolidaysDefinitions from "@/Components/Views/LabourHolidaysDefinitions.vue";
import LabourExceptionDefinitions from "@/Components/Views/LabourExceptionDefinitions.vue";
import EmploymentStatusDefinitions from "@/Components/Views/EmploymentStatusDefinitions.vue";
import LabourIndex from "@/Components/Views/LabourIndex.vue";
import AssignmentStatusDefinitions from "@/Components/Views/AssignmentStatusDefinitions.vue";
import ProjectStatusDefinitions from "@/Components/Views/ProjectStatusDefinitions.vue";
import ManHoursDefinitions from "@/Components/Views/ManHoursDefinitions.vue";
import QuotesIndex from "@/Components/Views/QuotesIndex.vue";
import Settings from "@/Components/Views/Settings.vue";
import AgentsDisplay from "@/Components/Views/AgentsDisplay.vue";
import LabourDisplay from "@/Components/Views/LabourDisplay.vue";
import AssignmentsDisplay from "@/Components/Views/AssignmentsDisplay.vue";
import ReportIndex from "@/Components/Views/ReportIndex.vue";
import DIDsIndex from "@/Components/Views/DIDsIndex.vue";
import DIDDisplay from "@/Components/Views/DIDDisplay.vue";
import CalendarsIndex from "@/Components/Views/CalendarsIndex.vue";
import CalendarDisplay from "@/Components/Views/CalendarDisplay.vue";
import OnCallAutoAttendantsIndex from "@/Components/Views/OnCallAutoAttendantsIndex.vue";
import OnCallAutoAttendantDisplay from "@/Components/Views/OnCallAutoAttendantDisplay.vue";
import VoicemailsIndex from "@/Components/Views/VoicemailsIndex.vue";
import VoicemailsDisplay from "@/Components/Views/VoicemailsDisplay.vue";
import RecordingsIndex from "@/Components/Views/RecordingsIndex.vue";

Vue.use(Router);

export default new Router({
	mode: "history",
	base: process.env.BASE_URL,
	routes: [
		{
			path: "/",
			name: "dashboard",
			component: Dashboard,
			meta: {
				title: "Dashboard - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Dashboard - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Dashboard - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/dashboard/agent",
			name: "agent-dashboard",
			component: DashboardAgent,
			meta: {
				title: "Agent Dashboard - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Agent Dashboard - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Agent Dashboard - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/dashboard/dispatch",
			name: "dispatch-dashboard",
			component: DashboardDispatch,
			meta: {
				title: "Dispatch Dashboard - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Dispatch Dashboard - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Dispatch Dashboard - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/dashboard/billing",
			name: "billing-dashboard",
			component: DashboardBilling,
			meta: {
				title: "Billing Dashboard - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Billing Dashboard - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Billing Dashboard - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/dashboard/management",
			name: "management-dashboard",
			component: DashboardManagement,
			meta: {
				title: "Management Dashboard - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Management Dashboard - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Management Dashboard - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/dashboard/on-call",
			name: "on-call-dashboard",
			component: DashboardOnCall,
			meta: {
				title: "On-Call Dashboard - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "On-Call Dashboard - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "On-Call Dashboard - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/contacts",
			redirect: "/section/contacts/index"
		},
		{
			path: "/section/contacts/index",
			name: "contacts-index",
			component: ContactsIndex,
			meta: {
				title: "Contacts - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Contacts - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Contacts - Dispatch Pulse"
					}
				]
			}
		},
		{
			path: "/section/contacts/:id",
			name: "contact-display",
			component: ContactDisplay,
			meta: {
				title: "Contact - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Contact - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Contact - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/companies",
			redirect: "/section/companies/index"
		},
		{
			path: "/section/companies/index",
			name: "companies-index",
			component: CompaniesIndex,
			meta: {
				title: "Companies - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Companies - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Companies - Dispatch Pulse"
					}
				]
			}
		},
		{
			path: "/section/companies/:id",
			name: "company-display",
			component: CompanyDisplay,
			meta: {
				title: "Company - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Company - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Company - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/projects",
			redirect: "/section/projects/index"
		},
		{
			path: "/section/projects/index",
			name: "projects-index",
			component: ProjectsIndex,
			meta: {
				title: "Projects - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Projects - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Projects - Dispatch Pulse"
					}
				]
			}
		},
		{
			path: "/section/projects/:id",
			name: "projects-display",
			component: ProjectsDisplay,
			meta: {
				title: "Project - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Project - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Project - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/agents",
			redirect: "/section/agents/index"
		},
		{
			path: "/section/agents/index",
			name: "agents-index",
			component: AgentsIndex,
			meta: {
				title: "Agents - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Agents - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Agents - Dispatch Pulse"
					}
				]
			}
		},
		{
			path: "/section/agents/:id",
			name: "agent-display",
			component: AgentsDisplay,
			meta: {
				title: "Agent - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Agent - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Agent - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/labour",
			redirect: "/section/labour/index"
		},
		{
			path: "/section/labour/index",
			name: "labour-index",
			component: LabourIndex,
			meta: {
				title: "Labour - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Labour - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Labour - Dispatch Pulse"
					}
				]
			}
		},
		{
			path: "/section/labour/:id",
			name: "labour-display",
			component: LabourDisplay,
			meta: {
				title: "Labour Entry - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Labour Entry - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Labour Entry - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/assignments",
			redirect: "/section/assignments/index"
		},
		{
			path: "/section/assignments/index",
			name: "assignments-index",
			component: AssignmentsIndex,
			meta: {
				title: "Assignments - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Assignments - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Assignments - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/assignments/:id",
			name: "assignment-display",
			component: AssignmentsDisplay,
			meta: {
				title: "Assignment Display - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Assignment Display - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Assignment Display - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/material-entries",
			redirect: "/section/material-entries/index"
		},
		{
			path: "/section/material-entries/index",
			name: "material-entries-index",
			component: MaterialsIndex,
			meta: {
				title: "Material Entries - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Material Entries - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Material Entries - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/quotes",
			redirect: "/section/quotes/index"
		},
		{
			path: "/section/quotes/index",
			name: "quotes-index",
			component: QuotesIndex,
			meta: {
				title: "Quotes - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Quotes - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Quotes - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/product-definitions",
			name: "product-definitions",
			component: ProductsDefinitions,
			meta: {
				title: "Product Definitions - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Product Definitions - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Product Definitions - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/assignment-status-definitions",
			name: "assignment-status-settings",
			component: AssignmentStatusDefinitions,
			meta: {
				title: "Assignment Status Definitions - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content:
							"Assignment Status Definitions - Dispatch Pulse"
					},
					{
						property: "og:description",
						content:
							"Assignment Status Definitions - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/man-hours-definitions",
			name: "man-hours-definitions",
			component: ManHoursDefinitions,
			meta: {
				title: "Man Hours Definitions - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Man Hours Definitions - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Man Hours Definitions - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/project-status-definitions",
			name: "project-status-definitions",
			component: ProjectStatusDefinitions,
			meta: {
				title: "Project Status Definitions - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Project Status Definitions - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Project Status Definitions - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/employment-status-definitions",
			name: "employment-status-settings",
			component: EmploymentStatusDefinitions,
			meta: {
				title: "Employment Statuses - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content:
							"Employment Status Definitions - Dispatch Pulse"
					},
					{
						property: "og:description",
						content:
							"Employment Status Definitions - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/labour-exception-definitions",
			name: "labour-exception-definitions",
			component: LabourExceptionDefinitions,
			meta: {
				title: "Labour Exception Definitions - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Labour Exception Definitions - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Labour Exception Definitions - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/labour-holiday-definitions",
			name: "labour-holidays-definitions",
			component: LabourHolidaysDefinitions,
			meta: {
				title: "Labour Holiday Definitions - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Labour Holiday Definitions - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Labour Holiday Definitions - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/labour-non-billable-definitions",
			name: "labour-non-billable-definitions",
			component: LabourNonBillableDefinitions,
			meta: {
				title: "Labour Non Billable Definitions - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content:
							"Labour Non Billable Definitions - Dispatch Pulse"
					},
					{
						property: "og:description",
						content:
							"Labour Non Billable Definitions - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/reports",
			redirect: "/section/reports/index"
		},
		{
			path: "/section/reports/index",
			name: "reports-index",
			component: ReportIndex,
			meta: {
				title: "Reports - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Reports - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Reports - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/dids",
			redirect: "/section/dids/index"
		},
		{
			path: "/section/dids/index",
			name: "dids-index",
			component: DIDsIndex,
			meta: {
				title: "Phone Numbers - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Phone Numbers - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Phone Numbers - Dispatch Pulse"
					}
				]
			}
		},
		{
			path: "/section/dids/:id",
			name: "did-display",
			component: DIDDisplay,
			meta: {
				title: "Inbound Phone Numbers - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Inbound Phone Numbers - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Inbound Phone Numbers - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/on-call",
			redirect: "/section/on-call/index"
		},
		{
			path: "/section/on-call/index",
			name: "on-call-index",
			component: OnCallAutoAttendantsIndex,
			meta: {
				title: "On Call Auto Attendants - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "On Call Auto Attendants - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "On Call Auto Attendants - Dispatch Pulse"
					}
				]
			}
		},
		{
			path: "/section/on-call/:id",
			name: "on-call-display",
			component: OnCallAutoAttendantDisplay,
			meta: {
				title: "On Call Responders - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "On Call Responders - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "On Call Responders - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/calendars",
			redirect: "/section/calendars/index"
		},
		{
			path: "/section/calendars/index",
			name: "calendars-index",
			component: CalendarsIndex,
			meta: {
				title: "Phone Numbers - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Phone Numbers - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Phone Numbers - Dispatch Pulse"
					}
				]
			}
		},
		{
			path: "/section/calendars/:id",
			name: "calendar-display",
			component: CalendarDisplay,
			meta: {
				title: "Calendar - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Calendar - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Calendar - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/voicemails",
			redirect: "/section/voicemails/index"
		},
		{
			path: "/section/voicemails/index",
			name: "voicemails-index",
			component: VoicemailsIndex,
			meta: {
				title: "Voicemails - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Voicemails - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Voicemails - Dispatch Pulse"
					}
				]
			}
		},
		{
			path: "/section/voicemails/:id",
			name: "voicemails-display",
			component: VoicemailsDisplay,
			meta: {
				title: "Voicemail - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Voicemail - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Voicemail - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/section/recordings",
			name: "recordings-index",
			component: RecordingsIndex,
			meta: {
				title: "Recordings - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Recordings - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Recordings - Dispatch Pulse"
					}
				]
			}
		},

		{
			path: "/settings",
			name: "settings",
			component: Settings,
			meta: {
				title: "Settings - Dispatch Pulse",
				metaTags: [
					{
						name: "description",
						content: "Settings - Dispatch Pulse"
					},
					{
						property: "og:description",
						content: "Settings - Dispatch Pulse"
					}
				]
			}
		},

		// A catch all so we don't just get a white page.
		{
			path: "*",
			redirect: "/"
		}
	],
	scrollBehavior(to, from, savedPosition) {
		if (savedPosition) {
			return savedPosition;
		}
		if (to.hash) {
			return { selector: to.hash };
		}
		return { x: 0, y: 0 };
	}
});
