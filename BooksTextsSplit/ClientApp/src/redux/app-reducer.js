import { getInit } from "./auth-reducer";
const INITIALIZED_SUCCESS = "INITIALIZED-SUCCESS";
const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";

let initialState = {
  initialized: false,
  isFetching: false,
  globalError: null,
  whoCalledPreloader: "",
};

const appReducer = (state = initialState, action) => {
  switch (action.type) {
    case INITIALIZED_SUCCESS:
      return {
        ...state,
        initialized: true,
      };
      case TOGGLE_IS_FETCHING: {
        if(action.isFetching){
        return { ...state, isFetching: action.isFetching, whoCalledPreloader: action.whoCalled };
        }
        else{
          return { ...state, isFetching: action.isFetching, whoCalledPreloader: "" };
        }
      }
    default:
      return state;
  }
};

export const initializedSuccess = () => ({ type: INITIALIZED_SUCCESS });

export const initializeApp = () => (dispatch) => {
  dispatch(toggleIsFetching(true));
  // TODO - Get all user data from Redis and return it to UI
  let promise = dispatch(getInit());  
  Promise.all([promise]).then(() => {
    dispatch(toggleIsFetching(false));
    dispatch(initializedSuccess());
  });
};

export const toggleIsFetching = (isFetching, whoCalled) => ({ type: TOGGLE_IS_FETCHING, isFetching, whoCalled });

export default appReducer;
