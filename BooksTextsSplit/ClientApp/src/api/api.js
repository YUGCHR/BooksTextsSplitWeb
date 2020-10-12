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

export const selectsAPI = {
  getAllBookIdsWithNames: (where, whereValue, startUpVersion) => {
    //api/BookTexts/BooksIds/?where=bookSentenceId&whereValue=1&orderBy=bookId&needPostSelect=true&postWhere=UploadVersion&postWhereValue=1
    //api/BookTexts/BooksIds/?where=bookSentenceId&whereValue=1&orderBy=bookId
    //return Axios.get(`api/BookTexts/BooksIds/?where=${where}&whereValue=${whereValue}&orderBy=${orderBy}`)
    //FromDbWhere/?where="bookSentenceId"&whereValue=1
    //return Axios.get(`api/BookTexts/BooksNamesIds/?where=${where}&whereValue=${whereValue}&startUploadVersion=${startUpVersion}`)
    return instance.get(`BooksNamesIds/?where=${where}&whereValue=${whereValue}&startUploadVersion=${startUpVersion}`).then((response) => {
      return response.data;
    });
  },
  //api/BookTexts/BookNameVersions/?where="bookId"&whereValue=1
  getAllBookNameVersions: (where, whereValue, bookId) => {
    return instance.get(`BookNameVersions/?where=${where}&whereValue=${whereValue}&bookId=${bookId}`).then((response) => {
      return response.data;
    });
  },
  // GET: api/BookTexts/BooksPairTexts/?where1=bookId&where1Value=(selected)&where2=uploadVersion&where2Value=(selected) - fetching selected version of the selected books pair texts
  getBooksPairTexts: (where1, where1Value, where2, where2Value) => {
    return instance
      .get(`BooksPairTexts/?where1=${where1}&where1Value=${where1Value}&where2=${where2}&where2Value=${where2Value}`)
      .then((response) => {
        return response.data;
      });
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
  getMe: (authKey) => {
    //debugger;
    // Alter defaults after instance has been created
    instance.defaults.headers.common['Authorization'] = authKey; //'Basic ' + 
    // headers must be - {'Authorization': 'Basic 1234567890'}
    return instance.get(`auth/getMe/`);
  },
  /* getMe() {
    return instance.get(`auth/me/`);
  }, */
  login: (email, password, rememberMe = false, captcha = null) => {
    return instance.post(`auth/login/`, { email, password, rememberMe, captcha });
  },
  logout: () => {
    return instance.delete(`auth/login/`);
  },
};

export const securityAPI = {
  getCaptchaUrl() {
    return instance.get(`security/get-captcha-url`);
  },
};
