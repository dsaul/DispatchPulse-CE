
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';


export default (val: string): boolean | string => {
	if (IsNullOrEmpty(val)) {
		return true;
	}
	
	if (val.length <= 13) {
		return 'Optional, but must be full 10 digit phone number.';
	} else {
		return true;
	}
};
