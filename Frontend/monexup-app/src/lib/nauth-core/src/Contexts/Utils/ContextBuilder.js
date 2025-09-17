import { jsx as _jsx } from "react/jsx-runtime";
const ContextBuilder = (providers) => {
    return providers.reduce((Accumulated, Current) => {
        return ({ children }) => (_jsx(Current, { children: _jsx(Accumulated, { children: children }) }));
    });
};
export default ContextBuilder;
