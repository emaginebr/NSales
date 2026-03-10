import React, { FC, PropsWithChildren } from 'react';

type ProviderComponent = FC<PropsWithChildren>;

const ContextBuilder = (providers: ProviderComponent[]): FC<PropsWithChildren> => {
  return providers.reduce((Accumulated, Current): FC<PropsWithChildren> => {
    return ({ children }) => (
      <Current>
        <Accumulated>{children}</Accumulated>
      </Current>
    );
  });
};

export default ContextBuilder;
