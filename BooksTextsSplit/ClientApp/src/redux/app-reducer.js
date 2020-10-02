import { getAuthUserData } from "./auth-reducer";
const INITIALIZED_SUCCESS = "INITIALIZED-SUCCESS";
const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";

let initialState = {
  initialized: false,
  isFetching: false,
  globalError: null,
};

const appReducer = (state = initialState, action) => {
  switch (action.type) {
    case INITIALIZED_SUCCESS:
      return {
        ...state,
        initialized: true,
      };

    case TOGGLE_IS_FETCHING:
      return {
        ...state,
        isFetching: action.isFetching,
      };
    default:
      return state;
  }
};

export const initializedSuccess = () => ({ type: INITIALIZED_SUCCESS });

export const initializeApp = () => (dispatch) => {
  dispatch(toggleIsFetching(true));
  let promise = dispatch(getAuthUserData());
  //dispatch();
  //dispatch();
  //Promise.all([promise]).then
  Promise.all([promise]).then(() => {
    dispatch(toggleIsFetching(false));
    dispatch(initializedSuccess());
  });
};

export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default appReducer;
