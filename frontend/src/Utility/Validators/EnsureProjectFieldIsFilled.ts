import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";

export default (val: string): boolean | string => {
	//console.log('EnsureProjectFieldIsFilled', val);

	return IsNullOrEmpty(val) ? "You must specify a project." : true;
};
