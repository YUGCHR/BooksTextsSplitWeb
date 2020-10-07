import { authAPI, securityAPI } from "../api/api";
import { stopSubmit } from "redux-form";
const SET_USER_DATA = "SET-USER-DATA";
const GET_CAPTCHA_URL_SUCCESS = "samurai-network/auth/GET-CAPTCHA-URL-SUCCESS";
const RESULT_CODE_NEEDS_CAPTCHA = 10;
const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";

let initialState = {
  userId: null,
  email: null,
  login: null,
  isAuth: false,
  isFetching: false,
  captchaUrl: null, // if null, then captcha is not required
};

const authReducer = (state = initialState, action) => {
  switch (action.type) {
    case SET_USER_DATA:
    case GET_CAPTCHA_URL_SUCCESS:
      return {
        ...state,
        ...action.payload,
        //isAuth: action.isAuth,
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

export const getCaptchaUrlSuccess = (captchaUrl) => ({ type: GET_CAPTCHA_URL_SUCCESS, payload: { captchaUrl } });

export const setAuthUserData = (userId, email, login, isAuth) => ({ type: SET_USER_DATA, payload: { userId, email, login, isAuth } });

export const getAuthUserData = () => async (dispatch) => {
  dispatch(toggleIsFetching(true));
  const response = await authAPI.getMe();
  dispatch(toggleIsFetching(false));
  debugger;
  if (response.data.resultCode === 0) {
    let allUsers = response.data.data;
    let user = allUsers[0];
    dispatch(setAuthUserData(user.id, user.email, user.login, true));
  }
};

export const login = (email, password, rememberMe, captcha) => async (dispatch) => {
  dispatch(toggleIsFetching(true));  
  const user = await authAPI.login(email, password, rememberMe, captcha);
  // login successful if there's a user in the response
  if (user) {
    // store user details and basic auth credentials in local storage
    // to keep user logged in between page refreshes
    user.authdata = window.btoa(email + ":" + password);
    localStorage.setItem("user", JSON.stringify(user));
    //return user;
  }
  dispatch(toggleIsFetching(false));  
  dispatch(getAuthUserData());
  /* if (response.data.resultCode === 0) {
    // success, get auth data
    dispatch(getAuthUserData());
  } else {
    if (response.data.resultCode === RESULT_CODE_NEEDS_CAPTCHA) {
      dispatch(getCaptchaUrl());
    }
    let errorDescription = response.data.messages.length > 0 ? response.data.messages[0] : "Something went wrong - please try again!";
    dispatch(stopSubmit("login", { _error: errorDescription }));
  } */
};

export const getCaptchaUrl = () => async (dispatch) => {
  const response = await securityAPI.getCaptchaUrl();
  const captchaUrl = response.data.url;
  dispatch(getCaptchaUrlSuccess(captchaUrl));
};

export const logout = () => async (dispatch) => {
  dispatch(toggleIsFetching(true));
  const response = await authAPI.logout();
  dispatch(toggleIsFetching(false));
  if (response.data.resultCode === 0) {
    dispatch(setAuthUserData(null, null, null, false));
    dispatch(getAuthUserData());
  }
};

function handleResponse(response) {
  let responseText = typeof response.data;

  return response.text().then((text) => {
    const data = text && JSON.parse(text);
    if (!response.ok) {
      if (response.status === 401) {
        // auto logout if 401 response returned from api
        logout();
        // eslint-disable-next-line no-restricted-globals
        location.reload(true);
      }

      const error = (data && data.message) || response.statusText;
      return Promise.reject(error);
    }

    return data;
  });
}

export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default authReducer;
