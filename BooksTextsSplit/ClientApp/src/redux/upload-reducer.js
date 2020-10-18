import { uploadAPI } from "../api/api";

const SET_DB_SENTENCES_COUNT = "SET-DB-SENTENCES-COUNT";
const SET_SENTENCES_COUNT = "SET-SENTENCES-COUNT";
const SET_FILE_NAME = "SET-FILE-NAME";
const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";
const TOGGLE_IS_LOADING = "TOGGLE-IS-LOADING";
const RADIO_IS_CHANGED = "RADIO-IS-CHANGED";
const FIND_MAX_UPLOADED = "FIND-MAX-UPLOADED";

let initialState = {
  selectedFiles: [
    { name: "eng", languageId: 8, bookId: 88, authorNameId: 88, authorName: "author", bookNameId: 88, bookName: "book" },
    { name: "rus", languageId: 8, bookId: 88, authorNameId: 88, authorName: "author", bookNameId: 88, bookName: "book" },
  ],
  selectedRadioLanguage: ["1", "2"],
  radioButtonsLabels: ["Book with English test", "Book with Russian test", "I do not know book language"],
  radioButtonsNames: ["radioEnglish", "radioRussian"],
  radioButtonsValues: ["1", "2", "3"],
  radioButtonsIds: [
    ["eng1", "eng2", "eng3"],
    ["rus1", "rus2", "rus3"],
  ],
  filesLanguageIds: [0, 1],
  uploading: false,
  uploadProgress: {},
  successfullUploaded: false,
  booksTitles: [
    [
      { bookId: 55, languageId: 0, authorNameId: 101, authorName: "1 Vernor Vinge", bookNameId: 1001, bookName: "1 A Fire Upon the Deep" },
      { bookId: 2, languageId: 0, authorNameId: 102, authorName: "2 Vernor Vinge", bookNameId: 1002, bookName: "2 A Fire Upon the Deep" },
      { bookId: 3, languageId: 0, authorNameId: 103, authorName: "3 Vernor Vinge", bookNameId: 1003, bookName: "3 A Fire Upon the Deep" },
      {
        bookId: 4,
        languageId: 0,
        authorNameId: 104,
        authorName: "4 Vernor Vinge",
        bookNameId: 1004,
        bookName: "4 A Fire Upon the Deep",
      },
      {
        bookId: 5,
        languageId: 0,
        authorNameId: 105,
        authorName: "5 Vernor Vinge",
        bookNameId: 1005,
        bookName: "5 A Fire Upon the Deep",
      },
    ],
    [
      { bookId: 55, languageId: 1, authorNameId: 101, authorName: "1 Вернор Виндж", bookNameId: 1001, bookName: "1 Пламя над бездной" },
      { bookId: 2, languageId: 1, authorNameId: 102, authorName: "2 Вернор Виндж", bookNameId: 1002, bookName: "2 Пламя над бездной" },
      { bookId: 3, languageId: 1, authorNameId: 103, authorName: "3 Вернор Виндж", bookNameId: 1003, bookName: "3 Пламя над бездной" },
      {
        bookId: 4,
        languageId: 1,
        authorNameId: 104,
        authorName: "4 Вернор Виндж",
        bookNameId: 1004,
        bookName: "4 Пламя над бездной",
      },
      {
        bookId: 5,
        languageId: 1,
        authorNameId: 105,
        authorName: "5 Вернор Виндж",
        bookNameId: 1001,
        bookName: "5 Пламя над бездной",
      },
    ],
  ],
  engSentences: [],
  lastSentenceNumber: null,
  rusSentences: [],
  sentencesOnPageTop: 10,
  sentencesCount: [-1, -2, -3, -4, -5],
  dbSentencesCount: [-7, -8], //engSentencesCount: 777, rusSentencesCount: 888
  emptyVariable: null,
  isTextLoaded: [false, false],
  creativeArrayLanguageId: [0, 1], //engLanguageId = 0; rusLanguageId = 1;
  bookTitle: [
    { languageId: 0, authorName: "1", bookTitle: "1" },
    { languageId: 1, authorName: "1", bookTitle: "1" },
  ],
  buttonsTextsParts: ["Load English Text -/", "Load Russian Text -/"],
  loadedTextTitle: ["You loaded English book --> ", "You loaded Russian book--> "],
  isFetching: false,
  uploadedVersions: [],
  maxUploadedVersion: -1,
};

const uploadBooksReducer = (state = initialState, action) => {
  switch (action.type) {
    case TOGGLE_IS_LOADING: {
      /* return { ...state, isEngLoaded: action.isEngLoaded } */
      let stateCopy = { ...state };
      stateCopy.isTextLoaded = { ...state.isTextLoaded };
      stateCopy.isTextLoaded[action.languageId] = action.isTextLoaded;
      return stateCopy;
    }
    case SET_DB_SENTENCES_COUNT: {
      let stateCopy = { ...state };
      stateCopy.dbSentencesCount = { ...state.dbSentencesCount };
      stateCopy.dbSentencesCount[action.languageId] = action.count;
      return stateCopy;
    }
    case SET_SENTENCES_COUNT: {
      let stateCopy = { ...state };
      stateCopy.sentencesCount = { ...state.sentencesCount };
      stateCopy.sentencesCount[action.index] = action.count;
      return stateCopy;
    }
    case SET_FILE_NAME: {
      return { ...state, selectedFiles: action.files };
    }
    case RADIO_IS_CHANGED: {
      //debugger;
      let stateCopy = { ...state };
      stateCopy.selectedRadioLanguage = { ...state.selectedRadioLanguage };
      stateCopy.selectedRadioLanguage[action.i] = action.option;
      stateCopy.filesLanguageIds = { ...state.filesLanguageIds };
      let languageId = parseInt(action.option) - 1;
      stateCopy.filesLanguageIds[action.i] = languageId;
      return stateCopy;
      //return { ...state, selectedRadioLanguage[action.languageId]: action.option };
    }
    case TOGGLE_IS_FETCHING: {
      return { ...state, isFetching: action.isFetching };
    }
    case FIND_MAX_UPLOADED: {
      let findMax = -1;
      action.uploadedVersions.map((u) => {
        if (u > findMax) {
          findMax = u;
        }
      });
      return { ...state, maxUploadedVersion: findMax };
    }
    default:
      return state;
  }
};

const setSentencesCount = (count, index) => ({ type: SET_SENTENCES_COUNT, count, index });
const toggleIsLoading = (isTextLoaded, languageId) => ({ type: TOGGLE_IS_LOADING, isTextLoaded, languageId });
const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export const setDbSentencesCount = (count, languageId) => ({ type: SET_DB_SENTENCES_COUNT, count, languageId });
export const setFileName = (files) => ({ type: SET_FILE_NAME, files });
export const radioOptionChange = (option, i) => ({ type: RADIO_IS_CHANGED, option, i });

const fetchLastUploadedVersions = (formData, i, selectedFiles) => async (dispatch, getState) => {
  dispatch(toggleIsFetching(true));
  let bookId = selectedFiles[i].bookId;
  let languageId = selectedFiles[i].languageId;
  const response = await uploadAPI.getLastUploadedVersions(bookId, languageId); // to find all previously uploaded versions of the file with this bookId
  dispatch(toggleIsFetching(false));
  formData.append("lastUploadedVersion", response.maxUploadedVersion);
  return formData;
};

const postBooksTexts = (formData, i) => async (dispatch, getState) => {
  dispatch(toggleIsFetching(true));
  const response = await uploadAPI.uploadFile(formData); //post returns response before all records have loaded in db
  dispatch(toggleIsFetching(false));
  //console.log(Response.data);
  dispatch(setSentencesCount(response, i)); //totalCount
  //console.log(this.props.sentencesCount[i]);
  const sentencesCountI = getState().uploadBooksPage.sentencesCount[i];
  return sentencesCountI;
};

export const fetchSentencesCount = (languageId) => async (dispatch, getState) => {
  dispatch(toggleIsFetching(true));
  const response = await uploadAPI.getSentenceCount(languageId);
  dispatch(toggleIsFetching(false));
  dispatch(setDbSentencesCount(response.sentencesCount, languageId));
  getState().uploadBooksPage.dbSentencesCount[languageId] === 0
    ? dispatch(toggleIsLoading(false, languageId))
    : dispatch(toggleIsLoading(true, languageId));
};

export const fileUploadHandler = (selectedFiles) => async (dispatch, getState) => {
  // it is possible to pass selectedFiles as parameter
  for (let i = 0; i < selectedFiles.length; i++) {
    const formData = new FormData();
    formData.append("bookFile", selectedFiles[i], selectedFiles[i].name);
    formData.append("languageId", selectedFiles[i].languageId); // it is possible to pass data in array instead file properties
    formData.append("bookId", selectedFiles[i].bookId);
    formData.append("authorNameId", selectedFiles[i].authorNameId);
    formData.append("authorName", selectedFiles[i].authorName);
    formData.append("bookNameId", selectedFiles[i].bookNameId);
    formData.append("bookName", selectedFiles[i].bookName);

    dispatch(fetchLastUploadedVersions(formData, i, selectedFiles)) // to add maxUploadedVersion to formData it is necessary to find it in Cosmos Db
      .then((formData) => {
        dispatch(postBooksTexts(formData, i));
      })
      .then((sentencesCount) => {
        if (sentencesCount < 0) {
          return -1;
        }
        dispatch(fetchSentencesCount(i));
        return getState().uploadBooksPage.dbSentencesCount[i];
      })
      .catch(failureCallback);
  }
};

const failureCallback = () => {
  //console.log(this.props.maxUploadedVersion);
};

export default uploadBooksReducer;