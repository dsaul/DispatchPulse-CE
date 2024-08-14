import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";

export default (val: string): string | boolean => {
	return IsNullOrEmpty(val) ? "Field is required." : true;
};
