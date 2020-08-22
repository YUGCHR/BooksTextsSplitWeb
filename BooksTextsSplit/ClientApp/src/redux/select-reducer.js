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

const setAllBookIdsWithNames = (bookNamesVersion1SortedByIds) => ({ type: SET_ALL_BOOKS_IDS, bookNamesVersion1SortedByIds });
const setAllVersionsOfBookName = (allVersionsOfBooksNames) => ({ type: SET_ALL_BOOKS_VERSIONS, allVersionsOfBooksNames });
const setBooksPairTexts = (booksPairTexts) => ({ type: SET_BOOKS_PAIR_TEXTS, booksPairTexts });
const toggleIsSelectingBookId = (isSelectingBookId) => ({ type: TOGGLE_IS_SELECTING_BOOK_ID, isSelectingBookId });
const toggleIsSelectingUploadVersion = (isSelectingUploadVersion) => ({ type: TOGGLE_IS_SELECTING_UPLOAD_VERSION, isSelectingUploadVersion });
const toggleIsQuickViewBooksPair = (isQuickViewBooksPair) => ({ type: TOGGLE_IS_QUICK_VIEW, isQuickViewBooksPair });
const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

const failureCallback = () => {
  console.log("FSE PROPALO!");
};

export const fetchAllBookIdsWithNames = (where = "bookSentenceId", whereValue = 1, startUpVersion = 1) => { 
  return (dispatch) => {
    dispatch(toggleIsFetching(true));
    return selectsAPI
      .getAllBookIdsWithNames(where, whereValue, startUpVersion)
      .then((data) => {
        dispatch(toggleIsFetching(false));
        dispatch(setAllBookIdsWithNames(data.bookNamesVersion1SortedByIds));
        return data.sortedBooksIdsLength;
      })
      .catch(failureCallback);
  };
};

const fetchAllVersionsOfSelectedBook = (where = "bookSentenceId", whereValue = 1, bookId) => {
  return (dispatch) => {
    dispatch(toggleIsFetching(true));
    return selectsAPI
      .getAllBookNameVersions(where, whereValue, bookId)
      .then((data) => {
        dispatch(toggleIsFetching(false));
        dispatch(setAllVersionsOfBookName(data.selectedBookIdAllVersions));
        return data;
      })
      .catch(failureCallback);
  };
};

const fetchChosenVersionOfSelectedBooksPair = (where1 = "bookId", where1Value, where2 = "uploadVersion", where2Value) => {  
  return (dispatch) => {
    dispatch(toggleIsFetching(true));
    return selectsAPI
      .getBooksPairTexts(where1, where1Value, where2, where2Value)
      .then((data) => {
        dispatch(toggleIsFetching(false));
        dispatch(setBooksPairTexts(data.selectedBooksPairTexts));
        return data;
      })
      .catch(failureCallback);
  };
};

//Let to switch on BooksNames choosing (return to the previous) - subPage 01
export const switchBooksIdsOn = () => {
  return (dispatch) => {
    dispatch(toggleIsSelectingUploadVersion(false)); //subPage 02 off
    dispatch(toggleIsQuickViewBooksPair(false)); //subPage  off
    dispatch(fetchAllBookIdsWithNames()).then((r) => {
      dispatch(toggleIsSelectingBookId(true)); //subPage 01 on
    });
    return 0;
  };
};

//Let to switch on BookVersions choosing - subPage 02
export const switchBookVersionsOn = (bookId) => {
  return (dispatch) => {
    dispatch(toggleIsSelectingBookId(false)); //subPage 01
    dispatch(toggleIsQuickViewBooksPair(false)); //subPage 03
    dispatch(fetchAllVersionsOfSelectedBook("bookSentenceId", 1, bookId)).then((r) => {
      dispatch(toggleIsSelectingUploadVersion(true)); //subPage 02
    });
    return { bookId };
  };
};

//Let to switch on QuickView - //subPage 03
export const switchQuickViewOn = (selectedBookId, selectedVersion) => {
  return (dispatch) => {
    dispatch(toggleIsSelectingBookId(false)); //subPage 01
    dispatch(toggleIsSelectingUploadVersion(false)); //subPage 02
    dispatch(fetchChosenVersionOfSelectedBooksPair("bookId", selectedBookId, "uploadVersion", selectedVersion)).then((r) => {
      dispatch(toggleIsQuickViewBooksPair(true)); //subPage 03
      return { selectedVersion };
    });
  };
};

//Let to switch on NEXT choosing - //subPage 04
export const nextAfterQuickView = (i) => {
  return (dispatch) => {
    dispatch(toggleIsSelectingUploadVersion(false)); //subPage 01
    dispatch(toggleIsQuickViewBooksPair(false)); //subPage 02
    dispatch(toggleIsQuickViewBooksPair(false)); //subPage 03
    return { i };
  };
};

export default selectTextsReducer;
