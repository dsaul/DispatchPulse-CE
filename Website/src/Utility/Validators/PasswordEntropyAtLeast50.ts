
import PasswordEntropy from '@/Utility/PasswordEntropy';

export default (val: string): string | boolean => {
		
	const complexity = PasswordEntropy.Determine(val);
	
	return complexity < 50 ? `Password complexity is ${complexity}, the complexity must be at least 50.` : true;
};
