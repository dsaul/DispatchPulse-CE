define(["require", "exports", "luxon"], function (require, exports, luxon_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.Index = exports.Global = void 0;
    class Global {
        static Start() {
            console.debug('Hello world! Global');
            requestAnimationFrame(() => {
                const els = document.getElementsByClassName("convertToLocalTime");
                for (const e of els) {
                    const text = e.innerText;
                    const dt = luxon_1.DateTime.fromISO(text);
                    const formatted = dt.toLocaleString(luxon_1.DateTime.DATETIME_FULL_WITH_SECONDS);
                    e.innerText = formatted;
                    //console.debug(dt);
                }
                const els2 = document.getElementsByClassName("convertToLocalTimeJournalEntries");
                for (const e of els2) {
                    const text = e.innerText;
                    const dt = luxon_1.DateTime.fromISO(text);
                    const formatted = dt.toFormat("yyyy-MM-dd HH:mm:ss");
                    e.innerText = formatted;
                    //console.debug(dt);
                }
            });
        }
    }
    exports.Global = Global;
    class Index {
        static Start() {
            console.debug('Hello world! Index', luxon_1.DateTime);
        }
    }
    exports.Index = Index;
    window.DEBUG_Index = Index;
    exports.default = {};
});
//# sourceMappingURL=Website.js.map