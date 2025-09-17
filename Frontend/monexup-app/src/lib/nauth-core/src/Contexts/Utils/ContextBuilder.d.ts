import { FC, PropsWithChildren } from 'react';
type ProviderComponent = FC<PropsWithChildren>;
declare const ContextBuilder: (providers: ProviderComponent[]) => FC<PropsWithChildren>;
export default ContextBuilder;
