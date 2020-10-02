import React, { Suspense } from "react";

export const withSuspense = (Component) => {
  return (props) => {
    return (
      <Suspense fallback={<div>LOADING...</div>}>
        <Component {...props} />
      </Suspense>
    );
  };
};
