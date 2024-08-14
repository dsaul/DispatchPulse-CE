

import store from '@/plugins/store/store';

export default (): boolean => {
	return store.state.demoMode;
};

