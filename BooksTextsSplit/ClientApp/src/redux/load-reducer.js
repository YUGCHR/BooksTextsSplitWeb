const SET_DB_SENTENCES_COUNT = "SET-DB-SENTENCES-COUNT";
const SET_SENTENCES_COUNT = "SET-SENTENCES-COUNT";
const SET_FILE_NAME = "SET-FILE-NAME";
const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";
const TOGGLE_IS_LOADING = "TOGGLE-IS-LOADING";
const RADIO_IS_CHANGED = "RADIO-IS-CHANGED";

let initialState = {
  selectedFiles: [{ name: "eng" }, { name: "rus" }],
  selectedRadioLanguage: ["1", "2"],
  radioButtonsLabels: [ "Book with English test", "Book with Russian test", "I do not know book language" ],
  radioButtonsNames: [ "radioEnglish", "radioRussian" ],
  radioButtonsValues: [ "1", "2", "3" ],
  radioButtonsIds: [ ["eng1", "eng2", "eng3"], ["rus1", "rus2", "rus3"] ],
  filesLanguageIds:[0, 1],
  uploading: false,
  uploadProgress: {},
  successfullUploaded: false,
  engTextTitle: [
    {
      languageId: 0,
      authorName: "1 Vernor Vinge",
      bookTitle: "1 A Fire Upon the Deep",
    },
    {
      languageId: 0,
      authorName: "2 Vernor Vinge",
      bookTitle: "2 A Fire Upon the Deep",
    },
    {
      languageId: 0,
      authorName: "3 Vernor Vinge",
      bookTitle: "3 A Fire Upon the Deep",
    },
    {
      languageId: 0,
      authorName: "4 Vernor Vinge",
      bookTitle: "4 A Fire Upon the Deep",
    },
    {
      languageId: 0,
      authorName: "5 Vernor Vinge",
      bookTitle: "5 A Fire Upon the Deep",
    },
  ],
  engSentences: [],
  lastSentenceNumber: null,
  rusTextTitle: [
    {
      languageId: 1,
      authorName: "1 Вернор Виндж",
      bookTitle: "1 Пламя над бездной",
    },
    {
      languageId: 1,
      authorName: "2 Вернор Виндж",
      bookTitle: "2 Пламя над бездной",
    },
    {
      languageId: 1,
      authorName: "3 Вернор Виндж",
      bookTitle: "3 Пламя над бездной",
    },
    {
      languageId: 1,
      authorName: "4 Вернор Виндж",
      bookTitle: "4 Пламя над бездной",
    },
    {
      languageId: 1,
      authorName: "5 Вернор Виндж",
      bookTitle: "5 Пламя над бездной",
    },
  ],
  rusSentences: [],
  sentencesOnPageTop: 10,
  sentencesCount: [111, 222, 333, 444, 555],
  dbSentencesCount: [777, 888], //engSentencesCount: 777, rusSentencesCount: 888
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
    default:
      return state;
  }
};

export const toggleIsLoading = (isTextLoaded, languageId) => ({ type: TOGGLE_IS_LOADING, isTextLoaded, languageId });
export const setDbSentencesCount = (count, languageId) => ({ type: SET_DB_SENTENCES_COUNT, count, languageId });
export const setSentencesCount = (count, index) => ({ type: SET_SENTENCES_COUNT, count, index });
export const setFileName = (files) => ({ type: SET_FILE_NAME, files });
export const radioOptionChange = (option, i) => ({ type: RADIO_IS_CHANGED, option, i });
export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default uploadBooksReducer;
