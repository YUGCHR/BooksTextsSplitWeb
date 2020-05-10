const SET_SENTENCES_COUNT = 'SET-SENTENCES-COUNT';
const SET_SENTENCES = 'SET-SENTENCES';
const SET_FILE_NAME = 'SET-FILE-NAME';
const TOGGLE_IS_FETCHING = 'TOGGLE-IS-FETCHING';
const TOGGLE_IS_LOADING = 'TOGGLE-IS-LOADING';

let initialState = {
    selectedFile: null,
    files: [],
    engSentences: [],
    uploading: false,
    uploadProgress: {},
    successfullUploaded: false,
    engTextTitle: [
        { languageId: 0, authorName: '1 Vernor Vinge', bookTitle: '1 A Fire Upon the Deep' },
        { languageId: 0, authorName: '2 Vernor Vinge', bookTitle: '2 A Fire Upon the Deep' },
        { languageId: 0, authorName: '3 Vernor Vinge', bookTitle: '3 A Fire Upon the Deep' },
        { languageId: 0, authorName: '4 Vernor Vinge', bookTitle: '4 A Fire Upon the Deep' },
        { languageId: 0, authorName: '5 Vernor Vinge', bookTitle: '5 A Fire Upon the Deep' },
    ],
    rusTextTitle: [
        { languageId: 1, authorName: '1 Вернор Виндж', bookTitle: '1 Пламя над бездной' },
        { languageId: 1, authorName: '2 Вернор Виндж', bookTitle: '2 Пламя над бездной' },
        { languageId: 1, authorName: '3 Вернор Виндж', bookTitle: '3 Пламя над бездной' },
        { languageId: 1, authorName: '4 Вернор Виндж', bookTitle: '4 Пламя над бездной' },
        { languageId: 1, authorName: '5 Вернор Виндж', bookTitle: '5 Пламя над бездной' }
    ],
    sentencesCount: [777, 888], //engSentencesCount: 777, rusSentencesCount: 888
    emptyVariable: null,
    isTextLoaded: [false, false],
    isFetching: false
}

const selectTextsReducer = (state = initialState, action) => {

    switch (action.type) {
        case TOGGLE_IS_LOADING:
            {
                /* return { ...state, isEngLoaded: action.isEngLoaded } */
                let stateCopy = {...state };
                stateCopy.isTextLoaded = {...state.isTextLoaded };
                stateCopy.isTextLoaded[action.languageId] = action.isTextLoaded;
                return stateCopy;
            }
        case SET_SENTENCES_COUNT:
            {
                let stateCopy = {...state };
                stateCopy.sentencesCount = {...state.sentencesCount };
                stateCopy.sentencesCount[action.languageId] = action.count;
                return stateCopy;
            }
            case SET_SENTENCES:
                {
                    let stateCopy = {...state };
                    stateCopy.engSentences = {...state.engSentences };
                    stateCopy.engSentences = action.sentences;
                    return stateCopy;
                }

        case SET_FILE_NAME:
            {
                let stateCopy = {...state };
                stateCopy.selectedFile = {...state.selectedFile };
                stateCopy.selectedFile = action.file;
                console.log(stateCopy.selectedFile);
                return stateCopy;
            }
        case TOGGLE_IS_FETCHING:
            {
                return {...state, isFetching: action.isFetching };
            }
        default:
            return state;
    }
}

export const toggleIsLoading = (isTextLoaded, languageId) => ({ type: TOGGLE_IS_LOADING, isTextLoaded, languageId });
export const setSentencesCount = (count, languageId) => ({ type: SET_SENTENCES_COUNT, count, languageId });
export const setSentences = (sentences, languageId) => ({ type: SET_SENTENCES, sentences, languageId });
export const setFileName = (file) => ({ type: SET_FILE_NAME, file });
export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default selectTextsReducer;