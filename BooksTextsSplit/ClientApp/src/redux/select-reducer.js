const SET_ALL_BOOKS_IDS = "SET-ALL-BOOKS-IDS";
const SET_ALL_BOOKS_VERSIONS = "SET-ALL-BOOKS-VERSIONS";
const SET_SENTENCES = "SET-SENTENCES";
const TOGGLE_IS_SELECTING_BOOK_ID = "TOGGLE-IS-SELECTING-BOOK-ID"
const TOGGLE_IS_SELECTING_UPLOAD_VERSION = "TOGGLE-IS-SELECTING-UPLOAD-VERSION"
const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";

let initialState = {
  bookNamesVersion1SortedByIds: [],
  allBookNamesSortedByIds: [],
  allVersionsOfBooksNames: [],
  sentencesCount: [777, 888], //engSentencesCount: 777, rusSentencesCount: 888
  emptyVariable: null,
  isTextLoaded: [false, false],
  isSelectingBookId: true,
  isSelectingUploadVersion: false,
  isFetching: false,
  gridContainerPlace2: "s.testGridContainerPlace2"
};

const selectTextsReducer = (state = initialState, action) => {
  switch (action.type) {
    case SET_ALL_BOOKS_IDS: {
      console.log('state', state.bookNamesVersion1SortedByIds);
      return { ...state, bookNamesVersion1SortedByIds: action.bookNamesVersion1SortedByIds };
    }
    case SET_ALL_BOOKS_VERSIONS:{
      return { ...state, allVersionsOfBooksNames: action.allVersionsOfBooksNames };
    }
    case SET_SENTENCES: {
      return { ...state, engSentences: action.sentences };
    }
    case TOGGLE_IS_SELECTING_BOOK_ID: {
      return { ...state, isSelectingBookId: action.isSelectingBookId, gridContainerPlace2: action.gridContainerPlace2 };
    }
    case TOGGLE_IS_SELECTING_UPLOAD_VERSION: {
      return { ...state, isSelectingUploadVersion: action.isSelectingUploadVersion, gridContainerPlace2: action.gridContainerPlace2 };
    }
    case TOGGLE_IS_FETCHING: {
      return { ...state, isFetching: action.isFetching };
    }
    default:
      return state;
  }
};

export const setAllBookIdsWithNames = (bookNamesVersion1SortedByIds) => ({ type: SET_ALL_BOOKS_IDS, bookNamesVersion1SortedByIds });
export const setAllVersionsOfBookName = (allVersionsOfBooksNames) => ({ type: SET_ALL_BOOKS_VERSIONS, allVersionsOfBooksNames });

export const setSentences = (sentences, languageId) => ({ type: SET_SENTENCES, sentences, languageId });

export const toggleIsSelectingBookId = (isSelectingBookId, gridContainerPlace2) => ({ type: TOGGLE_IS_SELECTING_BOOK_ID, isSelectingBookId, gridContainerPlace2 });
export const toggleIsSelectingUploadVersion = (isSelectingUploadVersion, gridContainerPlace2) => ({ type: TOGGLE_IS_SELECTING_UPLOAD_VERSION, isSelectingUploadVersion, gridContainerPlace2 });

export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default selectTextsReducer;
