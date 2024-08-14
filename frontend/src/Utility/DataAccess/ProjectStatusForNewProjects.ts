import { IProjectStatus } from "@/Data/CRM/ProjectStatus/ProjectStatus";
import store from "@/plugins/store/store";
import _ from "lodash";

export default (): string | null => {
	const projectStatuses: Record<string, IProjectStatus> =
		store.state.Database.projectStatus;
	if (!projectStatuses) {
		return null;
	}

	//console.log('projectStatuses', projectStatuses);

	const status = _.find(
		projectStatuses,
		o => o.json.isNewProjectStatus === true
	);
	if (!status) {
		return null;
	}

	if (!status.id) {
		return null;
	}

	return status.id;
};
