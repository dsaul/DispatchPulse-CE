// https://www.redips.net/javascript/array-move/

export default (arr: any[], pos1: number, pos2: number): void => {
	// if positions are different and inside array
	if (
		pos1 !== pos2 &&
		0 <= pos1 &&
		pos1 <= arr.length &&
		0 <= pos2 &&
		pos2 <= arr.length
	) {
		// save element from position 1
		const tmp = arr[pos1];
		// move element down and shift other elements up
		if (pos1 < pos2) {
			for (let i = pos1; i < pos2; i++) {
				arr[i] = arr[i + 1];
			}
		} else {
			// move element up and shift other elements down
			for (let i = pos1; i > pos2; i--) {
				arr[i] = arr[i - 1];
			}
		}
		// put element from position 1 to destination
		arr[pos2] = tmp;
	}
};
