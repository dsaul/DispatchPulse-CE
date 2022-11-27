import { DateTime } from 'luxon';

export class Global {
	public static Start(): void {
		console.debug('Hello world! Global');

		requestAnimationFrame(() => {
			const els: HTMLCollectionOf<HTMLElement> = document.getElementsByClassName("convertToLocalTime") as HTMLCollectionOf<HTMLElement>;
			for (const e of els) {

				const text = e.innerText;

				const dt = DateTime.fromISO(text);
				const formatted = dt.toLocaleString(DateTime.DATETIME_FULL_WITH_SECONDS);
				e.innerText = formatted;
				//console.debug(dt);
			}
			const els2: HTMLCollectionOf<HTMLElement> = document.getElementsByClassName("convertToLocalTimeJournalEntries") as HTMLCollectionOf<HTMLElement>;
			for (const e of els2) {
				const text = e.innerText;
				const dt = DateTime.fromISO(text);
				const formatted = dt.toFormat("yyyy-MM-dd HH:mm:ss");
				e.innerText = formatted;
				//console.debug(dt);
			}
		});

	}
}

export default {};
