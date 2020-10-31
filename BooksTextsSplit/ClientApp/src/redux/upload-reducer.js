import { act } from "react-test-renderer";
import { uploadAPI } from "../api/api";

const SET_DB_SENTENCES_COUNT = "SET-DB-SENTENCES-COUNT";
const SET_SENTENCES_COUNT = "SET-SENTENCES-COUNT";
const SET_FILE_NAME = "SET-FILE-NAME";
const SET_BOOKS_DESCRIPTIONS = "SET-BOOKS-DESCRIPTIONS";
const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";
const TOGGLE_IS_LOADING = "TOGGLE-IS-LOADING";
const TOGGLE_IS_DONE_UPLOAD = "TOGGLE-IS-DONE-UPLOAD";
const TOGGLE_IS_WRONG_COUNT = "TOGGLE-IS-WRONG-COUNT";
const TOGGLE_UPLOAD_BUTTON_ENABLE = "TOGGLE-UPLOAD-BUTTON-ENABLE";
const RADIO_DEFAULT = "RADIO-DEFAULT";
const RADIO_IS_CHANGED = "RADIO-IS-CHANGED";
const SHOW_HIDE_STATE = "SHOW-HIDE-STATE";
const FIND_MAX_UPLOADED = "FIND-MAX-UPLOADED";

let initialState = {
  selectedFiles: null, // used in ShowSelectedFiles
  radioChosenLanguage: ["eng", "rus"], // here default values of radio buttons to choose language
  radioAutoChangeLang: [
    ["eng", "rus"],
    ["rus", "eng"],
  ],
  radioAutoChangeLangInversed: [
    ["rus", "eng"],
    ["eng", "rus"],
  ],
  filesDescriptions: {
    index: "File No: ",
    name: "File name: ",
    lastMod: "Last modified: ",
    size: "File size: ",
    type: "File type: ",
    chosenLanguage: "Chosen file language: ",
  }, // used in ShowSelectedFiles
  uploadBooksLabels: {
    uploadBooksHeader1: "UPLOAD BOOKS ",
    uploadBooksHeader2: "CONTROL PANEL ",
    dbInfoHeader: "DB info",
    nearShowButton: " records, details - ",
    uploadButton: "Upload",
  },
  filesLanguageIds: [
    {
      languageId: 0,
      languageShortName: "eng",
      languagetName: "English",
    },
    {
      languageId: 1,
      languageShortName: "rus",
      languagetName: "Russian",
    },
  ],
  uploading: false,
  uploadProgress: {},
  successfullyUploaded: false,
  booksTitles: [{}, {}],
  engSentences: [],
  lastSentenceNumber: null,
  rusSentences: [],
  sentencesOnPageTop: 10,
  sentencesCount: [-1, -2, -3, -4, -5],
  dbSentencesCount: [-7, -8], //engSentencesCount: 777, rusSentencesCount: 888
  emptyVariable: null,
  isTextLoaded: [false, false],
  creativeArrayLanguageId: [0, 1], //engLanguageId = 0; rusLanguageId = 1;
  buttonsTextsParts: ["Load English Text -/", "Load Russian Text -/"],
  loadedTextTitle: ["You loaded English book --> ", "You loaded Russian book--> "],
  isFetching: false,
  uploadedVersions: [],
  maxUploadedVersion: -1,
  labelShowHide: [
    { label: "Show", value: false }, // value - are details shown
    { label: "Hide", value: true },
  ],
  isDoneUpload: false,
  isUploadButtonDisabled: true,
  isWrongCount: false,
  metadataHeader: "6L1n2qR1yzE0IjTZpUksGkbzF23vVGZeR0nEXL6qKhdXBGoJzSKqE9a1g",
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
    case SET_BOOKS_DESCRIPTIONS: {
      return { ...state, booksTitles: action.textsMetadata };
    }
    case RADIO_IS_CHANGED: {
      let stateCopy = { ...state };
      stateCopy.radioChosenLanguage = { ...state.radioChosenLanguage };
      stateCopy.booksTitles = { ...state.booksTitles };
      let viceVersaBooksTitles = false;
      if (action.chosenLang === "eng") {
        stateCopy.radioChosenLanguage = state.radioAutoChangeLang[action.i];
        // если неправильный languageId - получено "eng", а он = 1, меняем местами booksTitles.languageId
        if (stateCopy.booksTitles[action.i].languageId === 1) {
          viceVersaBooksTitles = true;
        }
      } else {
        stateCopy.radioChosenLanguage = state.radioAutoChangeLangInversed[action.i];
        if (stateCopy.booksTitles[action.i].languageId === 0) {
          viceVersaBooksTitles = true;
        }
      }
      if (viceVersaBooksTitles) {
        [stateCopy.booksTitles[0].languageId, stateCopy.booksTitles[1].languageId] = [
          stateCopy.booksTitles[1].languageId,
          stateCopy.booksTitles[0].languageId,
        ];
      }
      return stateCopy;
    }
    case RADIO_DEFAULT: {
      let stateCopy = { ...state };
      stateCopy.radioChosenLanguage = { ...state.radioChosenLanguage };
      state.filesLanguageIds.map((fli) => {
        if (action.defaultLanguageId === fli.languageId) {
          stateCopy.radioChosenLanguage[action.i] = fli.languageShortName;
        }
      });
      return stateCopy;
    }
    case SHOW_HIDE_STATE: {
      let stateCopy = { ...state };
      stateCopy.labelShowHide = { ...state.labelShowHide };
      let tempValue = state.labelShowHide[0];
      stateCopy.labelShowHide[0] = state.labelShowHide[1];
      stateCopy.labelShowHide[1] = tempValue;
      return stateCopy;
    }
    case TOGGLE_IS_FETCHING: {
      return { ...state, isFetching: action.isFetching };
    }
    case TOGGLE_IS_DONE_UPLOAD: {
      return { ...state, isDoneUpload: action.isDoneUpload };
    }
    case TOGGLE_UPLOAD_BUTTON_ENABLE: {
      return { ...state, isUploadButtonDisabled: action.isUploadButtonDisabled };
    }
    case TOGGLE_IS_WRONG_COUNT: {
      return { ...state, isWrongCount: action.isWrongCount };
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
const setDbSentencesCount = (count, languageId) => ({ type: SET_DB_SENTENCES_COUNT, count, languageId });

const toggleIsLoading = (isTextLoaded, languageId) => ({ type: TOGGLE_IS_LOADING, isTextLoaded, languageId });
const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });
const toggleIsDoneUpload = (isDoneUpload) => ({ type: TOGGLE_IS_DONE_UPLOAD, isDoneUpload });
const toggleUploadButtonDisable = (isUploadButtonDisabled) => ({ type: TOGGLE_UPLOAD_BUTTON_ENABLE, isUploadButtonDisabled });

const setFileName = (files) => ({ type: SET_FILE_NAME, files });
const setBooksDescriptions = (textsMetadata) => ({ type: SET_BOOKS_DESCRIPTIONS, textsMetadata });

const wrongFilesCountSelected = (isWrongCount) => ({ type: TOGGLE_IS_WRONG_COUNT, isWrongCount });
const setRadioDefault = (defaultLanguageId, i) => ({ type: RADIO_DEFAULT, defaultLanguageId, i });
export const setRadioResult = (chosenLang, i) => ({ type: RADIO_IS_CHANGED, chosenLang, i }); // used in ShowSelectedFiles
export const setShowHideState = (chosenLang, i) => ({ type: SHOW_HIDE_STATE, chosenLang, i }); // used in ShowSelectedFiles

const fetchLastUploadedVersions = (bookTitle) => async (dispatch, getState) => {
  dispatch(toggleIsFetching(true));
  const response = await uploadAPI.getLastUploadedVersions(bookTitle.bookId, bookTitle.languageId); // to find all previously uploaded versions of the file with this bookId
  dispatch(toggleIsFetching(false));
  bookTitle.uploadVersion = response.maxUploadedVersion;
  return bookTitle;
};

export const setFilesNamesAndEnableUpload = (files) => async (dispatch) => {
  //check user selected books pair
  dispatch(wrongFilesCountSelected(false));
  dispatch(toggleIsDoneUpload(false));
  if (files) {
    if (files.length === 2) {
      dispatch(setFileName(files));
      dispatch(toggleUploadButtonDisable(false));
      dispatch(setFilesMetadata(files));
    } else {
      dispatch(wrongFilesCountSelected(true));
    }
  } else {
    dispatch(setFileName(files));
    dispatch(toggleUploadButtonDisable(true));
  }
};

const setFilesMetadata = (files) => async (dispatch, getState) => {
  let textsMetadata = [{}, {}];
  for (let i = 0; i < files.length; i++) {
    //let file = files[i];
    let reader = new FileReader();
    reader.readAsText(files[i]);
    reader.onload = () => {
      // let textStrings = reader.result;
      const textFirst18Lines = reader.result.split("\n").slice(0, 18);
      //console.log(textFirst18Lines);
      if (textFirst18Lines[0].indexOf(getState().uploadBooksPage.metadataHeader) !== -1) {
        textsMetadata[i].bookId = parseInt(textFirst18Lines[2], 10);
        let currentLangId = parseInt(textFirst18Lines[4], 10);
        textsMetadata[i].languageId = currentLangId;
        dispatch(setRadioDefault(currentLangId, i));
        textsMetadata[i].authorNameId = parseInt(textFirst18Lines[6], 10);
        textsMetadata[i].authorName = textFirst18Lines[8];
        textsMetadata[i].bookNameId = parseInt(textFirst18Lines[10], 10);
        textsMetadata[i].bookName = textFirst18Lines[12];
        textsMetadata[i].comment = textFirst18Lines[14];
      }
    };
    reader.onerror = () => {
      console.log(reader.error);
    };
  }
  dispatch(setBooksDescriptions(textsMetadata));
};

const postBooksTexts = (formData, i) => async (dispatch) => {
  dispatch(toggleIsFetching(true));
  const response = await uploadAPI.uploadFile(formData); //post returns response before all records have loaded in db
  dispatch(toggleIsFetching(false));
  dispatch(setSentencesCount(response, i)); //totalCount
};

export const fetchSentencesCount = (languageId) => async (dispatch, getState) => {
  dispatch(toggleIsFetching(true));
  const response = await uploadAPI.getSentenceCount(languageId);
  dispatch(toggleIsFetching(false));
  dispatch(setDbSentencesCount(response.sentencesCount, languageId));
  getState().uploadBooksPage.dbSentencesCount[languageId] === 0
    ? dispatch(toggleIsLoading(false, languageId))
    : dispatch(toggleIsLoading(true, languageId));
  return response.sentencesCount;
};

export const fileUploadHandler = (selectedFiles) => async (dispatch, getState) => {
  dispatch(toggleUploadButtonDisable(true));
  for (let i = 0; i < selectedFiles.length; i++) {
    const form = new FormData();
    form.append("bookFile", selectedFiles[i], selectedFiles[i].name);
    // TODO it is possible to pass data in array (Object!) instead file properties
    const bookTitle = getState().uploadBooksPage.booksTitles[i]; //[0];
    //bookTitle.languageId = getState().uploadBooksPage.filesLanguageIds[i];
    dispatch(toggleIsFetching(true));
    // to add maxUploadedVersion to formData it is necessary to find it in Cosmos Db
    const bookTitleWithVersion = await dispatch(fetchLastUploadedVersions(bookTitle));
    const bookTitleWithVersionJson = JSON.stringify(bookTitleWithVersion);
    form.append("jsonBookDescription", bookTitleWithVersionJson);
    await dispatch(postBooksTexts(form, i));
    dispatch(toggleIsFetching(false));
    await dispatch(fetchSentencesCount(i)); // to fetch dbSentencesCount[languageId] and change toggleIsLoading on true
  }
  dispatch(toggleIsDoneUpload(true));
};

export default uploadBooksReducer;
