class Utility {
	public static DoPost(path, params, method = 'post') {

        // Doesn't work for any form that has CSRF

        const form = document.createElement('form');
        form.method = method;
        form.action = path;

        for (const key in params) {
            if (params.hasOwnProperty(key)) {
                const hiddenField = document.createElement('input');
                hiddenField.type = 'hidden';
                hiddenField.name = key;
                hiddenField.value = params[key];

                form.appendChild(hiddenField);
            }
        }

        document.body.appendChild(form);
        form.submit();

    }

    public static FormatMoney(amount) {
        return amount.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
    };

}


export default Utility;