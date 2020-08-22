import { selectsAPI } from "../api/api";

const SET_ALL_BOOKS_IDS = "SET-ALL-BOOKS-IDS";
const SET_ALL_BOOKS_VERSIONS = "SET-ALL-BOOKS-VERSIONS";
const SET_BOOKS_PAIR_TEXTS = "SET-BOOKS-PAIR-TEXTS";
const SET_SENTENCES = "SET-SENTENCES";
const TOGGLE_IS_SELECTING_BOOK_ID = "TOGGLE-IS-SELECTING-BOOK-ID";
const TOGGLE_IS_SELECTING_UPLOAD_VERSION = "TOGGLE-IS-SELECTING-UPLOAD-VERSION";
const TOGGLE_IS_QUICK_VIEW = "TOGGLE-IS-QUICK-VIEW";
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
  isQuickViewBooksPair: false,
  isFetching: false,
  //gridContainerPlace2: "s.testGridContainerPlace2",
  booksPairTexts: [],
};

const selectTextsReducer = (state = initialState, action) => {
  switch (action.type) {
    case SET_ALL_BOOKS_IDS: {
      console.log("state", state.bookNamesVersion1SortedByIds);
      return { ...state, bookNamesVersion1SortedByIds: action.bookNamesVersion1SortedByIds };
    }
    case SET_ALL_BOOKS_VERSIONS: {
      return { ...state, allVersionsOfBooksNames: action.allVersionsOfBooksNames };
    }
    case SET_BOOKS_PAIR_TEXTS: {
      return { ...state, booksPairTexts: action.booksPairTexts };
    }
    case SET_SENTENCES: {
      return { ...state, engSentences: action.sentences };
    }
    case TOGGLE_IS_SELECTING_BOOK_ID: {
      return { ...state, isSelectingBookId: action.isSelectingBookId }; // gridContainerPlace2: action.gridContainerPlace2 };
    }
    case TOGGLE_IS_SELECTING_UPLOAD_VERSION: {
      return { ...state, isSelectingUploadVersion: action.isSelectingUploadVersion }; // gridContainerPlace2: action.gridContainerPlace2 };
    }
    case TOGGLE_IS_QUICK_VIEW: {
      return { ...state, isQuickViewBooksPair: action.isQuickViewBooksPair };
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
export const setBooksPairTexts = (booksPairTexts) => ({ type: SET_BOOKS_PAIR_TEXTS, booksPairTexts });

export const setSentences = (sentences, languageId) => ({ type: SET_SENTENCES, sentences, languageId });

export const toggleIsSelectingBookId = (isSelectingBookId) => ({ type: TOGGLE_IS_SELECTING_BOOK_ID, isSelectingBookId });
export const toggleIsSelectingUploadVersion = (isSelectingUploadVersion) => ({ type: TOGGLE_IS_SELECTING_UPLOAD_VERSION, isSelectingUploadVersion });
export const toggleIsQuickViewBooksPair = (isQuickViewBooksPair) => ({ type: TOGGLE_IS_QUICK_VIEW, isQuickViewBooksPair });

export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

const failureCallback = () => {
  console.log("FSE PROPALO!");
};

export const fetchAllBookIdsWithNames = () => {
  let where = "bookSentenceId";
  let whereValue = 1;
  let startUpVersion = 1;
  return (dispatch) => {
    dispatch(toggleIsFetching(true));
    return selectsAPI.getAllBookIdsWithNames(where, whereValue, startUpVersion)
      .then((data) => {
        dispatch(toggleIsFetching(false));        
        dispatch(setAllBookIdsWithNames(data.bookNamesVersion1SortedByIds));        
        return data.sortedBooksIdsLength;
      })      
      .catch(failureCallback);
  };
};

export const fetchAllVersionsOfSelectedBook = (bookId) => {
  let where = "bookSentenceId";
  let whereValue = 1;
  return (dispatch) => {
    dispatch(toggleIsFetching(true));
    return selectsAPI.getAllBookNameVersions(where, whereValue, bookId)
      .then((data) => {
        dispatch(toggleIsFetching(false));        
        dispatch(setAllVersionsOfBookName(data.selectedBookIdAllVersions));        
        return data;
      })      
      .catch(failureCallback);
  };
};

export const fetchChosenVersionOfSelectedBooksPair = (selectedBookId, selectedVersion) => {
  let where1 = "bookId";
  let where1Value = selectedBookId;
  let where2 = "uploadVersion";
  let where2Value = selectedVersion;
  return (dispatch) => {
    dispatch(toggleIsFetching(true));
    return selectsAPI.getBooksPairTexts(where1, where1Value, where2, where2Value)
      .then((data) => {
        dispatch(toggleIsFetching(false));        
        dispatch(setBooksPairTexts(data.selectedBooksPairTexts));        
        return data;
      })      
      .catch(failureCallback);
  };
};

export default selectTextsReducer;
