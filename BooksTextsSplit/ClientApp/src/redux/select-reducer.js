import { selectsAPI } from "../api/api";
import { toggleIsFetching } from "./app-reducer";

const SET_ALL_BOOKS_IDS = "SET-ALL-BOOKS-IDS";
const SET_ALL_BOOKS_VERSIONS = "SET-ALL-BOOKS-VERSIONS";
const SET_BOOKS_PAIR_TEXTS = "SET-BOOKS-PAIR-TEXTS";
const SET_SENTENCES = "SET-SENTENCES";
const TOGGLE_IS_SELECT_BOOK_ID = "TOGGLE-IS-SELECTING-BOOK-ID";
const TOGGLE_IS_SELECT_VERSION = "TOGGLE-IS-SELECTING-UPLOAD-VERSION";
const TOGGLE_IS_QUICK_VIEW = "TOGGLE-IS-QUICK-VIEW";
//const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";

let initialState = {
  booksNamesIds: [],
  allBookNamesSortedByIds: [], //none
  allBookVersions: [],
  sentencesCount: [777, 888], //engSentencesCount: 777, rusSentencesCount: 888
  emptyVariable: null,
  isTextLoaded: [false, false],
  isSelectBookId: true,
  isSelectVersion: false,
  isQuickViewBooksPair: false,
  isFetching: false,
  //gridContainerPlace2: "s.testGridContainerPlace2",
  booksPairTexts: [],
};

const selectTextsReducer = (state = initialState, action) => {
  switch (action.type) {
    case SET_ALL_BOOKS_IDS: {
      // console.log("state", state.booksNamesIds);
      return { ...state, ...action.payload };
    }    
    case SET_ALL_BOOKS_VERSIONS: {
      return { ...state, allBookVersions: action.allBookVersions };
    }
    case SET_BOOKS_PAIR_TEXTS: {
      return { ...state, booksPairTexts: action.booksPairTexts };
    }
    case SET_SENTENCES: {
      return { ...state, engSentences: action.sentences };
    }
    case TOGGLE_IS_SELECT_BOOK_ID: {
      return { ...state, isSelectBookId: action.isSelectBookId }; // gridContainerPlace2: action.gridContainerPlace2 };
    }
    case TOGGLE_IS_SELECT_VERSION: {
      return { ...state, isSelectVersion: action.isSelectVersion }; // gridContainerPlace2: action.gridContainerPlace2 };
    }
    case TOGGLE_IS_QUICK_VIEW: {
      return { ...state, isQuickViewBooksPair: action.isQuickViewBooksPair };
    }
    /* case TOGGLE_IS_FETCHING: {
      return { ...state, isFetching: action.isFetching };
    } */
    default:
      return state;
  }
};

const setBooksNamesIds = (booksNamesIds) => ({ type: SET_ALL_BOOKS_IDS, payload: { booksNamesIds } });
const setBookVersions = (allBookVersions) => ({ type: SET_ALL_BOOKS_VERSIONS, allBookVersions });
const setBooksPairTexts = (booksPairTexts) => ({ type: SET_BOOKS_PAIR_TEXTS, booksPairTexts });
const toggleIsSelectBookId = (isSelectBookId) => ({ type: TOGGLE_IS_SELECT_BOOK_ID, isSelectBookId });
const toggleIsSelectVersion = (isSelectVersion) => ({ type: TOGGLE_IS_SELECT_VERSION, isSelectVersion });
const toggleIsQuickViewBooksPair = (isQuickViewBooksPair) => ({ type: TOGGLE_IS_QUICK_VIEW, isQuickViewBooksPair });
//const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

const failureCallback = () => {
  console.log("FSE PROPALO!");
};

const fetchBooksNamesIds = (where = "bookSentenceId", whereValue = 1, startUpVersion = 1) => async (dispatch) => {
  dispatch(toggleIsFetching(true, "fetchBooksNamesIds"));
  const response = await selectsAPI.getBooksNamesIds(where, whereValue, startUpVersion);
  dispatch(toggleIsFetching(false));
  dispatch(setBooksNamesIds(response.booksNamesIds)); //was bookNamesVersion1SortedByIds
};

const fetchAllVersionsOfSelectedBook = (where = "bookSentenceId", whereValue = 1, bookId) => {
  return (dispatch) => {
    dispatch(toggleIsFetching(true));
    return selectsAPI
      .getAllBookNameVersions(where, whereValue, bookId)
      .then((data) => {
        dispatch(toggleIsFetching(false));
        dispatch(setBookVersions(data.selectedBookIdAllVersions));
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
    dispatch(toggleIsSelectVersion(false)); //subPage 02 off
    dispatch(toggleIsQuickViewBooksPair(false)); //subPage  off
    dispatch(fetchBooksNamesIds()).then((r) => {
      dispatch(toggleIsSelectBookId(true)); //subPage 01 on
    });
    return 0;
  };
};

//Let to switch on BookVersions choosing - subPage 02
export const switchBookVersionsOn = (bookId = 1) => {
  return (dispatch) => {
    dispatch(toggleIsSelectBookId(false)); //subPage 01
    dispatch(toggleIsQuickViewBooksPair(false)); //subPage 03
    dispatch(fetchAllVersionsOfSelectedBook("bookSentenceId", 1, bookId)).then((r) => {
      dispatch(toggleIsSelectVersion(true)); //subPage 02
    });
    return { bookId };
  };
};

//Let to switch on QuickView - //subPage 03
export const switchQuickViewOn = (selectedBookId, selectedVersion) => {
  return (dispatch) => {
    dispatch(toggleIsSelectBookId(false)); //subPage 01
    dispatch(toggleIsSelectVersion(false)); //subPage 02
    dispatch(fetchChosenVersionOfSelectedBooksPair("bookId", selectedBookId, "uploadVersion", selectedVersion)).then((r) => {
      dispatch(toggleIsQuickViewBooksPair(true)); //subPage 03
      return { selectedVersion };
    });
  };
};

//Let to switch on NEXT choosing - //subPage 04
export const nextAfterQuickView = (i) => {
  return (dispatch) => {
    dispatch(toggleIsSelectVersion(false)); //subPage 01
    dispatch(toggleIsQuickViewBooksPair(false)); //subPage 02
    dispatch(toggleIsQuickViewBooksPair(false)); //subPage 03
    return { i };
  };
};

export default selectTextsReducer;

/* fail: Microsoft.AspNetCore.SpaServices[0]
      The prop value with an expression type of SequenceExpression could not be resolved. Please file issue to get this fixed immediately. */
