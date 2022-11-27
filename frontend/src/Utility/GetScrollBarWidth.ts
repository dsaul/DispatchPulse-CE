// https://stackoverflow.com/questions/13382516/getting-scroll-bar-width-using-javascript
export default (): number => {
	
	// Creating invisible container
	const outer = document.createElement('div');
	outer.style.visibility = 'hidden';
	outer.style.overflow = 'scroll'; // forcing scrollbar to appear
	(outer.style as any).msOverflowStyle = 'scrollbar'; // needed for WinJS apps
	document.body.appendChild(outer);
	
	// Creating inner element and placing it in the container
	const inner = document.createElement('div');
	outer.appendChild(inner);
	
	// Calculating difference between container's full width and the child width
	const scrollbarWidth = (outer.offsetWidth - inner.offsetWidth);
	
	// Removing temporary elements from the DOM
	if (outer && outer.parentNode) {
		outer.parentNode.removeChild(outer);
	}
	
	return scrollbarWidth;
};
