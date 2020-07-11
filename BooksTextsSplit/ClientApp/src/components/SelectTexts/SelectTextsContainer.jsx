import React from "react";
import Axios from "axios";
import ReactScrollWheelHandler from "react-scroll-wheel-handler";
import { connect } from "react-redux";
import { setAllBookIdsWithNames, setSentences, toggleIsFetching } from "../../redux/select-reducer";
import SelectTexts from "./SelectTexts";
import Preloader from "../common/preloader/Preloader";
class SelectTextsContainerAPI extends React.Component {
  constructor(props) {
    super(props);
  }

  componentDidMount() {
    this.fetchAllBookIdsWithNames();
  }

  fetchAllBookIdsWithNames = () => {
    //debugger;
    this.props.toggleIsFetching(true);
    let where = "bookSentenceId";
    let whereValue = 1;
    let orderBy = "bookId";
    return Axios.get(`api/BookTexts/BooksIds/?where=${where}&whereValue=${whereValue}&orderBy=${orderBy}`)
      .then((Response) => {
        this.props.toggleIsFetching(false);
        console.log(Response);        
        console.log('axios: sending this to props:', Response.data.allBookNamesSortedByIds)
        this.props.setAllBookIdsWithNames(Response.data.allBookNamesSortedByIds, Response.data.allEngBooksNames, Response.data.allRusBooksNames);
        console.log('axios: finished sending to props');
        let s = Response.data.sortedBooksIdsLength;
        //debugger;
        return s;
      })
      .catch(this.failureCallback);
  };  

  failureCallback = () => {
    console.log(this.props.maxUploadedVersion);
  };

  /* fetchSentences = (languageId) => {
        this.props.toggleIsFetching(true);
        Axios
            .get(`/api/BookTexts/count/${languageId}`)
            .then(Response => {                
                this.props.toggleIsFetching(false);
                this.props.setSentencesCount(Response.data.sentencesCount, languageId);            
                this.props.sentencesCount[languageId] === 0
                    ? this.props.toggleIsLoading(false, languageId)
                    : this.props.toggleIsLoading(true, languageId);
                
            });
        this.props.toggleIsFetching(true);
        debugger;
        Axios        
            .get(`/api/BookTexts/BookText/${languageId}`)
            .then(Response => {
                this.props.toggleIsFetching(false);
                this.props.setSentences(Response.data.sentences, languageId);
            });
    }
 */
  render() {

    console.log('container render starts', this.props.allBookNamesSortedByIds.length, this.props.allBookNamesSortedByIds);
    
    return (
      <>
        {this.props.isFetching ? <Preloader /> : null}
        <SelectTexts
          sentencesCount={this.props.sentencesCount}
          engSentences={this.props.engSentences}
          rusSentences={this.props.rusSentences}
          scrollLineUp={this.props.scrollLineUp}
          scrollLineDown={this.props.scrollLineDown}
          allBookNamesSortedByIds={this.props.allBookNamesSortedByIds}
          allEngBooksNames={this.props.allEngBooksNames}
          allRusBooksNames={this.props.allRusBooksNames}
          isFetching={this.props.isFetching}
          fetchAllBookIdsWithNames={this.fetchAllBookIdsWithNames}
        />
      </>
    );
  }
}

let mapStateToProps = (state) => {
  return {
    lastSentenceNumber: state.selectTextsPage.lastSentenceNumber,
    readingSentenceNumber: state.selectTextsPage.readingSentenceNumber,
    sentencesOnPageTop: state.selectTextsPage.sentencesOnPageTop,
    sentencesOnPageBottom: state.selectTextsPage.sentencesOnPageBottom,
    sentencesCount: state.selectTextsPage.sentencesCount,
    isFetching: state.uploadBooksPage.isFetching,
    allBookNamesSortedByIds: state.selectTextsPage.allBookNamesSortedByIds,
    allEngBooksNames: state.selectTextsPage.allEngBooksNames,
    allRusBooksNames: state.selectTextsPage.allRusBooksNames
  };
};

let SelectTextsContainer = connect(mapStateToProps, { setAllBookIdsWithNames, setSentences, toggleIsFetching })(SelectTextsContainerAPI);

export default SelectTextsContainer;
