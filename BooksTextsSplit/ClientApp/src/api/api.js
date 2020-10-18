import Axios from "axios";
//import { authHeader } from "../components/common/utils/authHeader";

// DAL - data access layer
//let header = {'Authorization': 'Basic '};

const instance = Axios.create({
  baseURL: `api/BookTexts/`,
  withCredentials: true,
});
// withCredentials: true,
// headers: {'Authorization': 'Basic 123456789'}
// headers: { "API-KEY": "6dd517b6-826d-4942-ab0a-022445b74fcd" }

export const uploadAPI = {
  getSentenceCount: async (languageId) => {
    const response = await instance.get(`count/${languageId}`);
    return response.data;
  },

  getLastUploadedVersions: async (bookId, languageId) => {
    const response = await instance.get(`BookUploadVersion/?bookId=${bookId}&languageId=${languageId}`);
    return response.data;
  },

  uploadFile: async (formData) => {
    const response = await instance.post(`UploadFile`, formData);
    return response.data;
  },
};

export const selectsAPI = {
  getAllBookIdsWithNames: async (where, whereValue, startUpVersion) => {
    //api/BookTexts/BooksIds/?where=bookSentenceId&whereValue=1&orderBy=bookId&needPostSelect=true&postWhere=UploadVersion&postWhereValue=1
    //api/BookTexts/BooksIds/?where=bookSentenceId&whereValue=1&orderBy=bookId
    //return Axios.get(`api/BookTexts/BooksIds/?where=${where}&whereValue=${whereValue}&orderBy=${orderBy}`)
    //FromDbWhere/?where="bookSentenceId"&whereValue=1
    //return Axios.get(`api/BookTexts/BooksNamesIds/?where=${where}&whereValue=${whereValue}&startUploadVersion=${startUpVersion}`)
    const response = await instance.get(`BooksNamesIds/?where=${where}&whereValue=${whereValue}&startUploadVersion=${startUpVersion}`);
    return response.data;
  },
  //api/BookTexts/BookNameVersions/?where="bookId"&whereValue=1
  getAllBookNameVersions: async (where, whereValue, bookId) => {
    const response = await instance.get(`BookNameVersions/?where=${where}&whereValue=${whereValue}&bookId=${bookId}`);
    return response.data;
  },
  // GET: api/BookTexts/BooksPairTexts/?where1=bookId&where1Value=(selected)&where2=uploadVersion&where2Value=(selected) - fetching selected version of the selected books pair texts
  getBooksPairTexts: async (where1, where1Value, where2, where2Value) => {
    const response = await instance.get(`BooksPairTexts/?where1=${where1}&where1Value=${where1Value}&where2=${where2}&where2Value=${where2Value}`);
    return response.data;
  },
};

export const failureCallback = () => {
  console.log(this.props.maxUploadedVersion);
};

/* const instanceAuth = Axios.create({
  // withCredentials: true,
  baseURL: `api/BookTexts/`,
  headers: authHeader(),
}); */

export const authAPI = {
  getInit: () => {
    return instance.get(`auth/init/`);
  },
  getMe: (authKey) => {
    // Alter defaults after instance has been created
    instance.defaults.headers.common["Authorization"] = authKey; //'Basic ' +
    // headers must be - {'Authorization': 'Basic 1234567890'}
    return instance.get(`auth/getMe/`);
    /* instance.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response.status === 401) {
          // dispatch something to your store
        }

        return Promise.reject(error);
      }
    ); */
  },
  login: (email, password, rememberMe = false, captcha = null) => {
    return instance.post(`auth/login/`, { email, password, rememberMe, captcha });
  },
  logout: () => {
    return instance.delete(`auth/logout/`);
  },
};

export const securityAPI = {
  getCaptchaUrl() {
    return instance.get(`security/get-captcha-url`);
  },
};

/* export const fetchCurrentUser = () => {
  return async (dispatch) => {
    try {
      const res = await instance.get(`auth/getMe/`);
      if (res.status === 200) {
        dispatch({});
      }
    } catch (error) {
      if (error.response.status === 401) {
        dispatch({});
      }
    }
  };
}; */
