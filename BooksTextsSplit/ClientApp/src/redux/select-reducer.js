const SET_ALL_BOOKS_IDS = "SET-ALL-BOOKS-IDS";
const SET_SENTENCES = "SET-SENTENCES";
const TOGGLE_IS_SELECTING_BOOK_ID = "TOGGLE-IS-SELECTING-BOOK-ID"
const TOGGLE_IS_SELECTING_UPLOAD_VERSION = "TOGGLE-IS-SELECTING-UPLOAD-VERSION"
const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";

let initialState = {
  allBookNamesSortedByIds: [],
  allEngBooksNames: [],
  allRusBooksNames: [],
  sentencesCount: [777, 888], //engSentencesCount: 777, rusSentencesCount: 888
  emptyVariable: null,
  isTextLoaded: [false, false],
  isSelectingBookId: true,
  isSelectingUploadVersion: false,
  isFetching: false,  
};

const selectTextsReducer = (state = initialState, action) => {
  switch (action.type) {
    case SET_ALL_BOOKS_IDS: {
      // console.log('state', state.allBookIdsWithNames);
      // console.log('copy', stateCopy.allBookIdsWithNames);
      return { ...state, allBookNamesSortedByIds: action.allBookNamesSortedByIds, allEngBooksNames: action.allEngBooksNames, allRusBooksNames: action.allRusBooksNames };
    }
    case SET_SENTENCES: {
      return { ...state, engSentences: action.sentences };
    }
    case TOGGLE_IS_SELECTING_BOOK_ID: {
      return { ...state, isSelectingBookId: action.isSelectingBookId };
    }
    case TOGGLE_IS_SELECTING_UPLOAD_VERSION: {
      return { ...state, isSelectingUploadVersion: action.isSelectingUploadVersion };
    }
    case TOGGLE_IS_FETCHING: {
      return { ...state, isFetching: action.isFetching };
    }
    default:
      return state;
  }
};

export const setAllBookIdsWithNames = (allBookNamesSortedByIds, allEngBooksNames, allRusBooksNames) => ({ type: SET_ALL_BOOKS_IDS, allBookNamesSortedByIds, allEngBooksNames, allRusBooksNames });
export const setSentences = (sentences, languageId) => ({ type: SET_SENTENCES, sentences, languageId });
export const toggleIsSelectingBookId = (isSelectingBookId) => ({ type: TOGGLE_IS_SELECTING_BOOK_ID, isSelectingBookId });
export const toggleIsSelectingUploadVersion = (isSelectingUploadVersion) => ({ type: TOGGLE_IS_SELECTING_UPLOAD_VERSION, isSelectingUploadVersion });
export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default selectTextsReducer;
