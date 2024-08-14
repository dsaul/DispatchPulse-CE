import { guid } from "@/Utility/GlobalTypes";

export interface IMessagePrompt {
	type: "polly" | "recording" | null;
	text: string | null;
	recordingId: guid | null;
}

export default {};
