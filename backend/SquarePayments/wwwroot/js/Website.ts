import { DateTime } from 'luxon';
import Utility from './Utility.js';

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
		});

	}
}

export default {};
