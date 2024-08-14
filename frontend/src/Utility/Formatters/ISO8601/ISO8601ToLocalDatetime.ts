import { DateTime } from "luxon";

export default (iso: string | null): string | null => {
	if (!iso) {
		return null;
	}

	const date = DateTime.fromISO(iso);

	return date.toLocaleString(DateTime.DATETIME_MED);
};
