export default interface IHubResponse {
	isError: boolean;
	errorMessage: string;
	isPermissionsError: boolean;
	forceLogout: boolean;
}
