import React, {Component} from 'react';
import {Header, Icon, List} from 'semantic-ui-react';
import logo from './logo.svg';
import './App.css';
import axios from 'axios';

class App extends Component {
  state={
     values:[]
  }
  componentDidMount(){
    axios.get('http://localhost:5000/values')
    .then((response)=>{
      console.log(response);
      this.setState({
        values:response.data
      })
    })
  }
  render(){
  return (
    <div className="App">
    <Header as='h2'>
      <Icon name='users'/>
    <Header.Content>Reactivities</Header.Content>
    </Header>
    <List>
    {this.state.values.map((value:any)=>(
            <li key={value.id}>{value.name}</li>
          ))}
    </List>
    </div>
  );
}
}

export default App;
